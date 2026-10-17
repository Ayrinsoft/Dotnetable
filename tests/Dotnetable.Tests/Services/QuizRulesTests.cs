using Dotnetable.Application.DTOs;
using Dotnetable.Application.Quizzes;
using Dotnetable.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Dotnetable.Tests.Services;

public class QuizRulesTests
{
    [Fact]
    public void SelectQuestionIds_Random_DrawsTheRequestedCount()
    {
        var ids = Enumerable.Range(1, 100).ToList();
        var drawn = QuizRules.SelectQuestionIds(ids, QuizQuestionMode.Random, 50, new Random(1));
        drawn.Should().HaveCount(50);
        drawn.Should().OnlyContain(id => ids.Contains(id));
        drawn.Distinct().Should().HaveCount(50);
    }

    [Fact]
    public void SelectQuestionIds_All_KeepsAuthorOrder()
    {
        var ids = new[] { 4, 9, 2 };
        QuizRules.SelectQuestionIds(ids, QuizQuestionMode.All, 1, new Random(1)).Should().Equal(4, 9, 2);
    }

    [Fact]
    public void Grade_SingleChoice_RequiresTheKey()
    {
        var questions = new[]
        {
            new QuizRules.GradeQuestion(1, 2, false, true, new HashSet<int> { 10 }),
            new QuizRules.GradeQuestion(2, 3, true, true, new HashSet<int> { 20, 21 }),
            new QuizRules.GradeQuestion(3, 5, false, false, new HashSet<int>()),
        };
        var answers = new List<QuizAnswerInput>
        {
            new() { QuizQuestionID = 1, OptionIds = { 10 } },
            new() { QuizQuestionID = 2, OptionIds = { 20 } },
            new() { QuizQuestionID = 3, Text = "notes" },
        };

        var grade = QuizRules.GradeAttempt(questions, answers);
        grade.Score.Should().Be(2);
        grade.MaxScore.Should().Be(5);
        grade.Percent.Should().Be(40);
    }

    [Fact]
    public void Grade_MultiChoice_NeedsTheExactSet()
    {
        var questions = new[]
        {
            new QuizRules.GradeQuestion(1, 4, true, true, new HashSet<int> { 1, 2 }),
        };
        var exact = QuizRules.GradeAttempt(questions, new List<QuizAnswerInput>
        {
            new() { QuizQuestionID = 1, OptionIds = { 2, 1 } },
        });
        exact.Score.Should().Be(4);

        var extra = QuizRules.GradeAttempt(questions, new List<QuizAnswerInput>
        {
            new() { QuizQuestionID = 1, OptionIds = { 1, 2, 3 } },
        });
        extra.Score.Should().Be(0);
    }

    [Fact]
    public void Validate_RequiresTheAnswerKeyWhenTheQuestionIsSaved()
    {
        var model = ValidQuiz();
        model.Questions[0].Options[0].IsCorrect = false;
        QuizRules.ValidateQuiz(model).Should().Contain("answer key");
    }

    [Fact]
    public void MatchBand_PicksTheNarrowestRange()
    {
        var bands = new List<QuizBandInput>
        {
            new() { MinPercent = 0, MaxPercent = 100, Title = "Overall", Body = "wide", SortOrder = 0 },
            new() { MinPercent = 0, MaxPercent = 40, Title = "Low", Body = "low", SortOrder = 1 },
        };
        QuizRules.MatchBand(bands, 20)!.Title.Should().Be("Low");
    }

    private static QuizEditorDto ValidQuiz() => new()
    {
        Title = "Personality",
        QuizCategoryID = 1,
        DurationMinutes = 20,
        QuestionMode = (byte)QuizQuestionMode.All,
        IsPublished = true,
        Questions =
        {
            new QuizQuestionInput
            {
                Prompt = "Pick one",
                Points = 1,
                IsActive = true,
                Options =
                {
                    new QuizOptionInput { Text = "A", IsCorrect = true },
                    new QuizOptionInput { Text = "B" },
                },
            },
        },
    };
}
