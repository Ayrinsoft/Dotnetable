using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Quizzes;

/// <summary>Pure rules for a timed test: what may be saved, which questions a sitting shows, and the score.</summary>
public static class QuizRules
{
    public const int MaxDurationMinutes = 24 * 60;

    public sealed record GradeQuestion(
        int QuizQuestionID,
        decimal Points,
        bool AllowMultiple,
        bool HasOptions,
        IReadOnlySet<int> CorrectOptionIds);

    public sealed record Grade(
        decimal Score,
        decimal MaxScore,
        decimal Percent,
        IReadOnlyDictionary<int, decimal> PointsByQuestion);

    public static string? ValidateCategory(QuizCategoryEditorDto model)
    {
        if (string.IsNullOrWhiteSpace(model.Title)) return "Title is required.";
        if (model.Title.Trim().Length > 200) return "Title is too long.";
        if (model.ParentCategoryID == model.QuizCategoryID && model.QuizCategoryID != 0)
            return "A category cannot be its own parent.";
        return null;
    }

    public static string? ValidateQuiz(QuizEditorDto model)
    {
        if (string.IsNullOrWhiteSpace(model.Title)) return "Title is required.";
        if (model.QuizCategoryID <= 0) return "Pick a category.";
        if (model.DurationMinutes < 1 || model.DurationMinutes > MaxDurationMinutes)
            return "Duration must be between 1 minute and 24 hours.";
        if (model.PrivateReviewFee < 0) return "The private-answer fee cannot be negative.";
        if (model.OfferPrivateReview && model.PrivateReviewFee > 0 && string.IsNullOrWhiteSpace(model.CurrencyCode))
            return "Set the currency for the private-answer fee.";

        var active = model.Questions.Where(q => q.IsActive).ToList();
        if (model.QuestionMode == (byte)QuizQuestionMode.Random)
        {
            if (model.RandomCount is null or < 1) return "Set how many questions to draw at random.";
            if (active.Count > 0 && model.RandomCount > active.Count)
                return "The random count cannot be higher than the number of active questions.";
        }

        if (model.IsPublished && active.Count == 0)
            return "Publish the test only after it has at least one active question.";

        for (var i = 0; i < model.Questions.Count; i++)
        {
            var error = ValidateQuestion(model.Questions[i], i + 1);
            if (error is not null) return error;
        }

        for (var i = 0; i < model.Bands.Count; i++)
        {
            var band = model.Bands[i];
            if (string.IsNullOrWhiteSpace(band.Title) || string.IsNullOrWhiteSpace(band.Body))
                return $"Score result {i + 1} needs a title and a text.";
            if (band.MinPercent < 0 || band.MaxPercent > 100 || band.MinPercent > band.MaxPercent)
                return $"Score result {i + 1} must be a percent range from 0 to 100.";
        }

        return null;
    }

    public static string? ValidateQuestion(QuizQuestionInput question, int number)
    {
        if (string.IsNullOrWhiteSpace(question.Prompt)) return $"Question {number} needs text.";
        if (question.Points <= 0) return $"Question {number} needs a score greater than zero.";

        var options = question.Options.Where(o => !string.IsNullOrWhiteSpace(o.Text)).ToList();
        if (options.Count == 0)
        {
            if (!question.AllowText) return $"Question {number} needs options or a written answer.";
            return null;
        }

        if (options.Count < 2) return $"Question {number} needs at least two options.";
        var correct = options.Count(o => o.IsCorrect);
        if (correct == 0) return $"Question {number} needs its answer key.";
        if (!question.AllowMultiple && correct != 1)
            return $"Question {number} is single-choice, so mark exactly one correct option.";
        return null;
    }

    /// <summary>
    /// Active question ids in author order, or a random sample of <paramref name="randomCount"/>.
    /// The sample is shuffled so two sittings do not share an order.
    /// </summary>
    public static IReadOnlyList<int> SelectQuestionIds(
        IReadOnlyList<int> orderedActiveIds, QuizQuestionMode mode, int? randomCount, Random random)
    {
        if (orderedActiveIds.Count == 0) return orderedActiveIds;
        if (mode != QuizQuestionMode.Random) return orderedActiveIds;

        var count = Math.Clamp(randomCount ?? orderedActiveIds.Count, 1, orderedActiveIds.Count);
        return orderedActiveIds.OrderBy(_ => random.Next()).Take(count).ToList();
    }

    /// <summary>
    /// Choice questions score only when the selected set is exactly the answer key.
    /// A written answer is stored for the specialist and does not change the automatic score.
    /// </summary>
    public static Grade GradeAttempt(IReadOnlyList<GradeQuestion> questions, IReadOnlyList<QuizAnswerInput> answers)
    {
        var byQuestion = answers
            .GroupBy(a => a.QuizQuestionID)
            .ToDictionary(g => g.Key, g => g.Last());

        var points = new Dictionary<int, decimal>();
        decimal score = 0;
        decimal max = 0;

        foreach (var question in questions)
        {
            if (!question.HasOptions) continue;
            max += question.Points;
            byQuestion.TryGetValue(question.QuizQuestionID, out var answer);
            var selected = (answer?.OptionIds ?? new List<int>()).Where(id => id > 0).ToHashSet();
            var exact = selected.SetEquals(question.CorrectOptionIds);
            if (!question.AllowMultiple && selected.Count != 1) exact = false;
            var awarded = exact ? question.Points : 0;
            points[question.QuizQuestionID] = awarded;
            score += awarded;
        }

        var percent = max <= 0 ? 0 : Math.Round(score / max * 100m, 2, MidpointRounding.AwayFromZero);
        return new Grade(score, max, percent, points);
    }

    public static QuizBandInput? MatchBand(IReadOnlyList<QuizBandInput> bands, decimal percent) =>
        bands
            .Where(b => percent >= b.MinPercent && percent <= b.MaxPercent)
            .OrderBy(b => b.MaxPercent - b.MinPercent)
            .ThenBy(b => b.SortOrder)
            .FirstOrDefault();
}
