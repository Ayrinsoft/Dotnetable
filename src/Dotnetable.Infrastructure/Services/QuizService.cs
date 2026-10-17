using System.Net;
using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Quizzes;
using Dotnetable.Application.Text;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Services;

public class QuizService : IQuizService
{
    private static readonly TimeSpan SubmitGrace = TimeSpan.FromMinutes(2);

    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IPaymentGatewayProviderRegistry _gateways;
    private readonly IOnlinePaymentService _onlinePayments;
    private readonly ICurrencyConversionService _money;
    private readonly IEmailService _email;
    private readonly IAdminNotificationService _notifications;
    private readonly ILogger<QuizService> _logger;

    public QuizService(
        IDbContextFactory<AppDbContext> contextFactory,
        IPaymentGatewayProviderRegistry gateways,
        IOnlinePaymentService onlinePayments,
        ICurrencyConversionService money,
        IEmailService email,
        IAdminNotificationService notifications,
        ILogger<QuizService> logger)
    {
        _contextFactory = contextFactory;
        _gateways = gateways;
        _onlinePayments = onlinePayments;
        _money = money;
        _email = email;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task<List<QuizCategoryListItemDto>> GetCategoriesAsync(int? websiteId, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var query = db.QuizCategories.AsNoTracking().AsQueryable();
        if (websiteId is int id) query = query.Where(c => c.WebsiteID == id);
        return await query
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Title)
            .Select(c => new QuizCategoryListItemDto
            {
                QuizCategoryID = c.QuizCategoryID,
                WebsiteID = c.WebsiteID,
                Title = c.Title,
                Slug = c.Slug,
                IsActive = c.IsActive,
                ShowInPosts = c.ShowInPosts,
                SortOrder = c.SortOrder,
                QuizCount = c.Quizzes.Count,
            })
            .ToListAsync(ct);
    }

    public async Task<QuizCategoryEditorDto?> GetCategoryAsync(int categoryId, int? websiteId, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var category = await db.QuizCategories.AsNoTracking()
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.QuizCategoryID == categoryId && (websiteId == null || c.WebsiteID == websiteId), ct);
        if (category is null) return null;
        return new QuizCategoryEditorDto
        {
            QuizCategoryID = category.QuizCategoryID,
            WebsiteID = category.WebsiteID,
            ParentCategoryID = category.ParentCategoryID,
            Title = category.Title,
            Slug = category.Slug,
            Description = category.Description,
            SortOrder = category.SortOrder,
            IsActive = category.IsActive,
            ShowInPosts = category.ShowInPosts,
            Translations = category.Translations.Select(t => new QuizTextDto
            {
                LanguageCode = t.LanguageCode,
                Title = t.Title,
                Body = t.Description,
            }).ToList(),
        };
    }

    public async Task<(bool Ok, string? Error, int Id)> SaveCategoryAsync(QuizCategoryEditorDto model, CancellationToken ct = default)
    {
        var error = QuizRules.ValidateCategory(model);
        if (error is not null) return (false, error, 0);

        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        if (model.ParentCategoryID is int parentId)
        {
            var parentOk = await db.QuizCategories.AnyAsync(
                c => c.QuizCategoryID == parentId && c.WebsiteID == model.WebsiteID, ct);
            if (!parentOk) return (false, "The parent category is not on this website.", 0);
        }

        var slug = await UniqueCategorySlugAsync(db, model, ct);
        QuizCategory category;
        if (model.QuizCategoryID == 0)
        {
            category = new QuizCategory { WebsiteID = model.WebsiteID, CreatedAt = DateTime.UtcNow };
            db.QuizCategories.Add(category);
        }
        else
        {
            var existing = await db.QuizCategories.Include(c => c.Translations)
                .FirstOrDefaultAsync(c => c.QuizCategoryID == model.QuizCategoryID && c.WebsiteID == model.WebsiteID, ct);
            if (existing is null) return (false, "Category not found.", 0);
            category = existing;
            db.QuizCategoryTranslations.RemoveRange(category.Translations);
            category.Translations.Clear();
        }

        category.ParentCategoryID = model.ParentCategoryID;
        category.Title = model.Title.Trim();
        category.Slug = slug;
        category.Description = Clean(model.Description);
        category.SortOrder = model.SortOrder;
        category.IsActive = model.IsActive;
        category.ShowInPosts = model.ShowInPosts;
        foreach (var text in model.Translations.Where(t => !string.IsNullOrWhiteSpace(t.LanguageCode)))
        {
            if (string.IsNullOrWhiteSpace(text.Title) && string.IsNullOrWhiteSpace(text.Body)) continue;
            category.Translations.Add(new QuizCategoryTranslation
            {
                LanguageCode = text.LanguageCode.Trim().ToLowerInvariant(),
                Title = Clean(text.Title),
                Description = Clean(text.Body),
            });
        }

        await db.SaveChangesAsync(ct);
        return (true, null, category.QuizCategoryID);
    }

    public async Task<string?> DeleteCategoryAsync(int categoryId, int? websiteId, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var category = await db.QuizCategories
            .Include(c => c.Children)
            .Include(c => c.Quizzes)
            .FirstOrDefaultAsync(c => c.QuizCategoryID == categoryId && (websiteId == null || c.WebsiteID == websiteId), ct);
        if (category is null) return "Category not found.";
        if (category.Children.Count > 0 || category.Quizzes.Count > 0)
            return "Move or delete the tests and child categories first.";
        db.QuizCategories.Remove(category);
        await db.SaveChangesAsync(ct);
        return null;
    }

    public async Task<List<QuizListItemDto>> GetQuizzesAsync(int? websiteId, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var query = db.Quizzes.AsNoTracking().AsQueryable();
        if (websiteId is int id) query = query.Where(q => q.WebsiteID == id);
        return await query
            .OrderByDescending(q => q.QuizID)
            .Select(q => new QuizListItemDto
            {
                QuizID = q.QuizID,
                WebsiteID = q.WebsiteID,
                QuizCategoryID = q.QuizCategoryID,
                CategoryTitle = q.QuizCategory.Title,
                Title = q.Title,
                Slug = q.Slug,
                DurationMinutes = q.DurationMinutes,
                QuestionMode = q.QuestionMode,
                RandomCount = q.RandomCount,
                QuestionCount = q.Questions.Count(x => x.IsActive),
                IsPublished = q.IsPublished,
                OfferPrivateReview = q.OfferPrivateReview,
                AttemptCount = q.Attempts.Count,
            })
            .ToListAsync(ct);
    }

    public async Task<QuizEditorDto?> GetQuizAsync(int quizId, int? websiteId, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var quiz = await db.Quizzes.AsNoTracking()
            .Include(q => q.Translations)
            .Include(q => q.ScoreBands).ThenInclude(b => b.Translations)
            .Include(q => q.Questions).ThenInclude(x => x.Options)
            .Include(q => q.Questions).ThenInclude(x => x.Images).ThenInclude(i => i.FileRecord)
            .FirstOrDefaultAsync(q => q.QuizID == quizId && (websiteId == null || q.WebsiteID == websiteId), ct);
        if (quiz is null) return null;

        return new QuizEditorDto
        {
            QuizID = quiz.QuizID,
            WebsiteID = quiz.WebsiteID,
            QuizCategoryID = quiz.QuizCategoryID,
            Title = quiz.Title,
            Slug = quiz.Slug,
            Summary = quiz.Summary,
            DurationMinutes = quiz.DurationMinutes,
            QuestionMode = quiz.QuestionMode,
            RandomCount = quiz.RandomCount,
            IsPublished = quiz.IsPublished,
            OfferPrivateReview = quiz.OfferPrivateReview,
            PrivateReviewFee = quiz.PrivateReviewFee,
            CurrencyCode = quiz.CurrencyCode,
            Translations = quiz.Translations.Select(t => new QuizTextDto
            {
                LanguageCode = t.LanguageCode, Title = t.Title, Body = t.Summary,
            }).ToList(),
            Bands = quiz.ScoreBands.OrderBy(b => b.SortOrder).Select(b => new QuizBandInput
            {
                QuizScoreBandID = b.QuizScoreBandID,
                MinPercent = b.MinPercent,
                MaxPercent = b.MaxPercent,
                Title = b.Title,
                Body = b.Body,
                SortOrder = b.SortOrder,
                Translations = b.Translations.Select(t => new QuizTextDto
                {
                    LanguageCode = t.LanguageCode, Title = t.Title, Body = t.Body,
                }).ToList(),
            }).ToList(),
            Questions = quiz.Questions.OrderBy(x => x.SortOrder).Select(x => new QuizQuestionInput
            {
                QuizQuestionID = x.QuizQuestionID,
                Prompt = x.Prompt,
                Points = x.Points,
                AllowMultiple = x.AllowMultiple,
                AllowText = x.AllowText,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                Options = x.Options.OrderBy(o => o.SortOrder).Select(o => new QuizOptionInput
                {
                    QuizOptionID = o.QuizOptionID,
                    Text = o.Text,
                    IsCorrect = o.IsCorrect,
                    SortOrder = o.SortOrder,
                }).ToList(),
                Images = x.Images.OrderBy(i => i.SortOrder).Select(i => new QuizImageInput
                {
                    FileRecordID = i.FileRecordID,
                    Url = i.FileRecord.CNDUrl ?? i.FileRecord.ThumbnailCDN,
                    SortOrder = i.SortOrder,
                }).ToList(),
            }).ToList(),
        };
    }

    public async Task<(bool Ok, string? Error, int Id)> SaveQuizAsync(QuizEditorDto model, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        if (string.IsNullOrWhiteSpace(model.CurrencyCode))
        {
            model.CurrencyCode = await db.Websites.Where(w => w.WebsiteID == model.WebsiteID)
                .Select(w => w.DefaultCurrencyCode).FirstOrDefaultAsync(ct);
        }

        var error = QuizRules.ValidateQuiz(model);
        if (error is not null) return (false, error, 0);

        var categoryOk = await db.QuizCategories.AnyAsync(
            c => c.QuizCategoryID == model.QuizCategoryID && c.WebsiteID == model.WebsiteID, ct);
        if (!categoryOk) return (false, "The category is not on this website.", 0);

        var fileError = await ValidateFilesAsync(db, model, ct);
        if (fileError is not null) return (false, fileError, 0);

        Quiz quiz;
        if (model.QuizID == 0)
        {
            quiz = new Quiz
            {
                WebsiteID = model.WebsiteID,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            db.Quizzes.Add(quiz);
        }
        else
        {
            quiz = await db.Quizzes
                .Include(q => q.Translations)
                .Include(q => q.ScoreBands).ThenInclude(b => b.Translations)
                .Include(q => q.Questions).ThenInclude(x => x.Options)
                .Include(q => q.Questions).ThenInclude(x => x.Images)
                .FirstOrDefaultAsync(q => q.QuizID == model.QuizID && q.WebsiteID == model.WebsiteID, ct)
                ?? throw new InvalidOperationException("Test not found.");
        }

        quiz.Slug = await UniqueQuizSlugAsync(db, model, ct);
        quiz.QuizCategoryID = model.QuizCategoryID;
        quiz.Title = model.Title.Trim();
        quiz.Summary = Clean(model.Summary);
        quiz.DurationMinutes = model.DurationMinutes;
        quiz.QuestionMode = model.QuestionMode;
        quiz.RandomCount = model.QuestionMode == (byte)QuizQuestionMode.Random ? model.RandomCount : null;
        quiz.IsPublished = model.IsPublished;
        quiz.OfferPrivateReview = model.OfferPrivateReview;
        quiz.PrivateReviewFee = model.OfferPrivateReview ? model.PrivateReviewFee : 0;
        quiz.CurrencyCode = string.IsNullOrWhiteSpace(model.CurrencyCode) ? null : model.CurrencyCode.Trim().ToUpperInvariant();
        quiz.UpdatedAt = DateTime.UtcNow;

        if (model.QuizID != 0)
        {
            var replaceError = await ReplaceChildrenAsync(db, quiz, model, ct);
            if (replaceError is not null) return (false, replaceError, 0);
        }
        else
        {
            AddChildren(quiz, model);
        }

        await db.SaveChangesAsync(ct);
        return (true, null, quiz.QuizID);
    }

    public async Task<string?> DeleteQuizAsync(int quizId, int? websiteId, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var quiz = await db.Quizzes.Include(q => q.Attempts)
            .FirstOrDefaultAsync(q => q.QuizID == quizId && (websiteId == null || q.WebsiteID == websiteId), ct);
        if (quiz is null) return "Test not found.";
        if (quiz.Attempts.Count > 0) return "This test already has sittings, so it cannot be deleted.";
        db.Quizzes.Remove(quiz);
        await db.SaveChangesAsync(ct);
        return null;
    }

    public async Task<List<QuizAttemptAdminDto>> GetAttemptsAsync(int quizId, int? websiteId, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var quizOk = await db.Quizzes.AnyAsync(q => q.QuizID == quizId && (websiteId == null || q.WebsiteID == websiteId), ct);
        if (!quizOk) return new List<QuizAttemptAdminDto>();

        var attempts = await db.QuizAttempts.AsNoTracking()
            .Where(a => a.QuizID == quizId)
            .OrderByDescending(a => a.QuizAttemptID)
            .Include(a => a.Answers).ThenInclude(x => x.SelectedOptions).ThenInclude(o => o.QuizOption)
            .Include(a => a.Answers).ThenInclude(x => x.QuizQuestion).ThenInclude(q => q.Options)
            .ToListAsync(ct);

        return attempts.Select(a => new QuizAttemptAdminDto
        {
            QuizAttemptID = a.QuizAttemptID,
            Token = a.PublicToken.ToString("N"),
            StartedAt = a.StartedAt,
            SubmittedAt = a.SubmittedAt,
            Score = a.Score,
            MaxScore = a.MaxScore,
            Percent = a.Percent,
            BandTitle = a.BandTitle,
            ContactName = a.ContactName,
            Phone = a.Phone,
            Email = a.Email,
            PrivateReviewStatus = a.PrivateReviewStatus,
            PrivateReviewFee = a.PrivateReviewFee,
            CurrencyCode = a.CurrencyCode,
            PrivateAnswer = a.PrivateAnswer,
            Answers = a.Answers.Select(answer => new QuizAttemptAdminAnswerDto
            {
                Prompt = answer.QuizQuestion.Prompt,
                TextAnswer = answer.TextAnswer,
                PointsAwarded = answer.PointsAwarded,
                Points = answer.QuizQuestion.Points,
                Selected = answer.SelectedOptions.Select(o => o.QuizOption.Text).ToList(),
                Correct = answer.QuizQuestion.Options.Where(o => o.IsCorrect).Select(o => o.Text).ToList(),
            }).ToList(),
        }).ToList();
    }

    public async Task<string?> MarkManualPaymentAsync(int attemptId, int? websiteId, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var attempt = await OwnedAttemptAsync(db, attemptId, websiteId, ct);
        if (attempt is null) return "Sitting not found.";
        if (attempt.PrivateReviewStatus is (byte)QuizPrivateReviewStatus.Delivered)
            return "The private answer was already sent.";
        if (attempt.PrivateReviewStatus is (byte)QuizPrivateReviewStatus.None)
            return "This person has not asked for a private answer.";

        attempt.PrivateReviewStatus = (byte)QuizPrivateReviewStatus.PaidAwaitingAnswer;
        attempt.PaidAt ??= DateTime.UtcNow;
        if (attempt.PaymentID is int paymentId)
        {
            var payment = await db.Payments.FirstOrDefaultAsync(p => p.PaymentID == paymentId, ct);
            if (payment is not null && payment.Status == (byte)PaymentStatus.Pending)
            {
                payment.Status = (byte)PaymentStatus.Paid;
                payment.PaidAt = DateTime.UtcNow;
                payment.Method = (byte)PaymentMethod.Manual;
            }
        }

        await db.SaveChangesAsync(ct);
        return null;
    }

    public async Task<string?> SendPrivateAnswerAsync(int attemptId, int? websiteId, string answer, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(answer)) return "Write the private answer first.";
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var attempt = await OwnedAttemptAsync(db, attemptId, websiteId, ct);
        if (attempt is null) return "Sitting not found.";
        if (attempt.PrivateReviewStatus != (byte)QuizPrivateReviewStatus.PaidAwaitingAnswer)
            return "Register the payment before sending the private answer.";
        if (string.IsNullOrWhiteSpace(attempt.Email)) return "This request has no email address.";

        var quizTitle = await db.Quizzes.Where(q => q.QuizID == attempt.QuizID).Select(q => q.Title).FirstAsync(ct);
        attempt.PrivateAnswer = answer.Trim();
        attempt.PrivateAnswerAt = DateTime.UtcNow;
        attempt.PrivateReviewStatus = (byte)QuizPrivateReviewStatus.Delivered;
        await db.SaveChangesAsync(ct);

        try
        {
            var body = WebUtility.HtmlEncode(attempt.PrivateAnswer).Replace("\n", "<br>");
            await _email.SendAsync(
                attempt.WebsiteID,
                EmailAccountType.Info,
                attempt.Email,
                quizTitle,
                $"<p>{WebUtility.HtmlEncode(attempt.ContactName)}</p><p>{body}</p>",
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Private quiz answer {AttemptId} was saved but the email was not sent.", attemptId);
            return "The answer is saved, but the email could not be sent. Check the site email account and send it again.";
        }

        return null;
    }

    public async Task<IReadOnlyList<QuizPublicCategoryDto>> GetPublicCategoriesAsync(
        int websiteId, string? lang, bool postsOnly, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var categories = await db.QuizCategories.AsNoTracking()
            .Include(c => c.Translations)
            .Include(c => c.Quizzes).ThenInclude(q => q.Translations)
            .Include(c => c.Quizzes).ThenInclude(q => q.Questions)
            .Where(c => c.WebsiteID == websiteId && c.IsActive && (!postsOnly || c.ShowInPosts))
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Title)
            .ToListAsync(ct);

        return categories.Select(c => ToPublicCategory(c, lang)).ToList();
    }

    public async Task<QuizPublicCategoryDto?> GetPublicCategoryAsync(int websiteId, int categoryId, string? lang, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var category = await db.QuizCategories.AsNoTracking()
            .Include(c => c.Translations)
            .Include(c => c.Quizzes).ThenInclude(q => q.Translations)
            .Include(c => c.Quizzes).ThenInclude(q => q.Questions)
            .FirstOrDefaultAsync(c => c.QuizCategoryID == categoryId && c.WebsiteID == websiteId && c.IsActive, ct);
        return category is null ? null : ToPublicCategory(category, lang);
    }

    public async Task<QuizPublicDetailDto?> GetPublicQuizAsync(int websiteId, int quizId, string? lang, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var quiz = await db.Quizzes.AsNoTracking()
            .Include(q => q.Translations)
            .Include(q => q.QuizCategory).ThenInclude(c => c.Translations)
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.QuizID == quizId && q.WebsiteID == websiteId && q.IsPublished && q.QuizCategory.IsActive, ct);
        if (quiz is null) return null;
        var active = quiz.Questions.Count(x => x.IsActive);
        return new QuizPublicDetailDto
        {
            QuizID = quiz.QuizID,
            Title = Pick(quiz.Title, quiz.Translations.Select(t => (t.LanguageCode, t.Title)), lang),
            Summary = PickOptional(quiz.Summary, quiz.Translations.Select(t => (t.LanguageCode, t.Summary)), lang),
            CategoryTitle = Pick(quiz.QuizCategory.Title, quiz.QuizCategory.Translations.Select(t => (t.LanguageCode, t.Title)), lang),
            CategoryId = quiz.QuizCategoryID,
            DurationMinutes = quiz.DurationMinutes,
            QuestionCount = quiz.QuestionMode == (byte)QuizQuestionMode.Random
                ? Math.Min(quiz.RandomCount ?? active, active)
                : active,
            QuestionMode = quiz.QuestionMode,
            RandomCount = quiz.RandomCount,
            OfferPrivateReview = quiz.OfferPrivateReview,
            PrivateReviewFee = quiz.PrivateReviewFee,
            CurrencyCode = quiz.CurrencyCode,
        };
    }

    public async Task<QuizActionResult> StartAttemptAsync(int websiteId, int quizId, int? clientId, string? lang, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var quiz = await db.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.QuizID == quizId && q.WebsiteID == websiteId && q.IsPublished, ct);
        if (quiz is null) return QuizActionResult.Fail("This test is not available.");

        var ordered = quiz.Questions.Where(q => q.IsActive).OrderBy(q => q.SortOrder).ThenBy(q => q.QuizQuestionID)
            .Select(q => q.QuizQuestionID).ToList();
        var picked = QuizRules.SelectQuestionIds(ordered, (QuizQuestionMode)quiz.QuestionMode, quiz.RandomCount, Random.Shared);
        if (picked.Count == 0) return QuizActionResult.Fail("This test has no questions yet.");

        var now = DateTime.UtcNow;
        var attempt = new QuizAttempt
        {
            QuizID = quiz.QuizID,
            WebsiteID = websiteId,
            WebsiteClientID = clientId,
            PublicToken = Guid.NewGuid(),
            StartedAt = now,
            ExpiresAt = now.AddMinutes(quiz.DurationMinutes),
            Status = (byte)QuizAttemptStatus.InProgress,
            LanguageCode = string.IsNullOrWhiteSpace(lang) ? null : lang.Trim().ToLowerInvariant(),
            PrivateReviewFee = quiz.OfferPrivateReview ? quiz.PrivateReviewFee : 0,
            CurrencyCode = quiz.CurrencyCode,
        };
        var order = 0;
        foreach (var questionId in picked)
        {
            attempt.Questions.Add(new QuizAttemptQuestion
            {
                QuizQuestionID = questionId,
                SortOrder = order++,
            });
        }

        db.QuizAttempts.Add(attempt);
        await db.SaveChangesAsync(ct);
        var view = await BuildViewAsync(db, attempt, ct);
        return QuizActionResult.Success(view);
    }

    public async Task<QuizAttemptViewDto?> GetAttemptAsync(int websiteId, Guid token, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var attempt = await db.QuizAttempts.FirstOrDefaultAsync(a => a.WebsiteID == websiteId && a.PublicToken == token, ct);
        if (attempt is null) return null;
        if (attempt.Status == (byte)QuizAttemptStatus.InProgress && DateTime.UtcNow > attempt.ExpiresAt.Add(SubmitGrace))
            await CloseAttemptAsync(db, attempt, new QuizSubmitRequest(), expired: true, ct);

        return await BuildViewAsync(db, attempt, ct);
    }

    public async Task<QuizActionResult> SubmitAttemptAsync(int websiteId, Guid token, QuizSubmitRequest request, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var attempt = await db.QuizAttempts.FirstOrDefaultAsync(a => a.WebsiteID == websiteId && a.PublicToken == token, ct);
        if (attempt is null) return QuizActionResult.Fail("This sitting was not found.");
        if (attempt.Status != (byte)QuizAttemptStatus.InProgress)
            return QuizActionResult.Success(await BuildViewAsync(db, attempt, ct));

        var expired = DateTime.UtcNow > attempt.ExpiresAt.Add(SubmitGrace);
        await CloseAttemptAsync(db, attempt, expired ? new QuizSubmitRequest() : request, expired || DateTime.UtcNow > attempt.ExpiresAt, ct);
        return QuizActionResult.Success(await BuildViewAsync(db, attempt, ct));
    }

    public async Task<QuizActionResult> RequestPrivateReviewAsync(
        int websiteId, Guid token, int? clientId, QuizPrivateReviewRequest request, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var attempt = await db.QuizAttempts.Include(a => a.Quiz)
            .FirstOrDefaultAsync(a => a.WebsiteID == websiteId && a.PublicToken == token, ct);
        if (attempt is null) return QuizActionResult.Fail("This sitting was not found.");
        if (attempt.Status == (byte)QuizAttemptStatus.InProgress)
            return QuizActionResult.Fail("Finish the test before asking for a private answer.");
        if (!attempt.Quiz.OfferPrivateReview)
            return QuizActionResult.Fail("This test does not offer a private answer.");
        if (attempt.PrivateReviewStatus == (byte)QuizPrivateReviewStatus.Delivered)
            return QuizActionResult.Success(await BuildViewAsync(db, attempt, ct));
        if (attempt.PrivateReviewStatus == (byte)QuizPrivateReviewStatus.PaidAwaitingAnswer)
            return QuizActionResult.Success(await BuildViewAsync(db, attempt, ct), message: "Your request is already with the specialist.");

        var contactError = ValidateContact(request);
        if (contactError is not null) return QuizActionResult.Fail(contactError);

        attempt.WantsPrivateReview = true;
        attempt.ContactName = request.FullName.Trim();
        attempt.Phone = request.Phone.Trim();
        attempt.Email = request.Email.Trim();
        attempt.WebsiteClientID ??= clientId;
        attempt.PrivateReviewFee = attempt.Quiz.PrivateReviewFee;
        attempt.CurrencyCode = attempt.Quiz.CurrencyCode;

        if (attempt.PrivateReviewFee <= 0)
        {
            attempt.PrivateReviewStatus = (byte)QuizPrivateReviewStatus.PaidAwaitingAnswer;
            attempt.PaidAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            await NotifyReviewAsync(attempt, "A test taker asked for a private answer.", ct);
            return QuizActionResult.Success(await BuildViewAsync(db, attempt, ct));
        }

        if (!request.PayOnline)
        {
            attempt.PrivateReviewStatus = (byte)QuizPrivateReviewStatus.ManualRequested;
            await db.SaveChangesAsync(ct);
            await NotifyReviewAsync(attempt, "A test taker asked to pay for a private answer in person.", ct);
            return QuizActionResult.Success(await BuildViewAsync(db, attempt, ct));
        }

        if (request.GatewayId is not int gatewayId)
            return QuizActionResult.Fail("Pick a payment method.");

        var started = await StartGatewayAsync(db, attempt, gatewayId, request, ct);
        if (!started.Ok) return started;
        await NotifyReviewAsync(attempt, "A test taker started online payment for a private answer.", ct);
        return started;
    }

    public async Task<QuizActionResult> CompletePrivatePaymentAsync(
        int websiteId, Guid token, GatewayCallback callback, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var attempt = await db.QuizAttempts.Include(a => a.PaymentGateway).Include(a => a.Payment)
            .FirstOrDefaultAsync(a => a.WebsiteID == websiteId && a.PublicToken == token, ct);
        if (attempt is null) return QuizActionResult.Fail("This sitting was not found.");
        if (attempt.PrivateReviewStatus is (byte)QuizPrivateReviewStatus.PaidAwaitingAnswer or (byte)QuizPrivateReviewStatus.Delivered)
            return QuizActionResult.Success(await BuildViewAsync(db, attempt, ct));
        if (attempt.PaymentGateway is null || string.IsNullOrWhiteSpace(attempt.GatewayAuthority))
            return QuizActionResult.Fail("There is no online payment to confirm.");

        if (!string.IsNullOrWhiteSpace(callback.Authority) &&
            !string.Equals(callback.Authority, attempt.GatewayAuthority, StringComparison.Ordinal))
            return QuizActionResult.Fail("This payment does not match the request.");

        var provider = _gateways.Find(attempt.PaymentGateway.Provider);
        if (provider is null) return QuizActionResult.Fail("The payment gateway is no longer available.");

        var verifyCallback = string.IsNullOrWhiteSpace(callback.Authority)
            ? callback with { Authority = attempt.GatewayAuthority }
            : callback;
        var result = await provider.VerifyAsync(ToContext(attempt.PaymentGateway), verifyCallback, ct);
        if (!result.Paid)
        {
            if (attempt.Payment is not null) attempt.Payment.Status = (byte)PaymentStatus.Failed;
            await db.SaveChangesAsync(ct);
            return QuizActionResult.Fail(result.Error ?? "The payment was not confirmed.");
        }

        if (result.PaidAmount is decimal charged && Math.Abs(charged - attempt.PrivateReviewFee) > 0.01m)
        {
            _logger.LogError("Quiz payment {AttemptId} was confirmed for {Charged}, expected {Expected}.",
                attempt.QuizAttemptID, charged, attempt.PrivateReviewFee);
            return QuizActionResult.Fail("The amount confirmed by the gateway does not match this request.");
        }

        MarkPaid(attempt, result.ReferenceNumber);
        await db.SaveChangesAsync(ct);
        return QuizActionResult.Success(await BuildViewAsync(db, attempt, ct));
    }

    private async Task<QuizActionResult> StartGatewayAsync(
        AppDbContext db, QuizAttempt attempt, int gatewayId, QuizPrivateReviewRequest request, CancellationToken ct)
    {
        var gateway = await db.PaymentGateways.FirstOrDefaultAsync(
            g => g.PaymentGatewayID == gatewayId && g.WebsiteID == attempt.WebsiteID && g.IsActive, ct);
        if (gateway is null) return QuizActionResult.Fail("That payment method is not available.");
        var provider = _gateways.Find(gateway.Provider);
        if (provider is null || !provider.IsConfigured(ToContext(gateway)))
            return QuizActionResult.Fail("That payment method is not configured.");
        if (string.IsNullOrWhiteSpace(request.ReturnUrl))
            return QuizActionResult.Fail("The return address is missing.");

        var clientId = attempt.WebsiteClientID ?? await FindClientIdAsync(db, attempt, ct);
        Payment? payment = null;
        if (clientId is int payerId)
        {
            decimal amountUsd;
            try { amountUsd = await _money.ToUsdAsync(attempt.WebsiteID, attempt.PrivateReviewFee, attempt.CurrencyCode, ct); }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Quiz fee for attempt {AttemptId} could not be converted to USD.", attempt.QuizAttemptID);
                amountUsd = attempt.PrivateReviewFee;
            }

            payment = new Payment
            {
                WebsiteID = attempt.WebsiteID,
                WebsiteClientID = payerId,
                Method = (byte)PaymentMethod.OnlineGateway,
                PaymentGatewayID = gateway.PaymentGatewayID,
                Amount = attempt.PrivateReviewFee,
                CurrencyCode = attempt.CurrencyCode ?? "",
                ExchangeRateToUsd = attempt.PrivateReviewFee == 0 ? 1 : amountUsd / attempt.PrivateReviewFee,
                AmountUsd = amountUsd,
                Status = (byte)PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
            };
            db.Payments.Add(payment);
            await db.SaveChangesAsync(ct);
            attempt.PaymentID = payment.PaymentID;
            attempt.WebsiteClientID = payerId;
        }

        var returnUrl = string.IsNullOrWhiteSpace(gateway.CallbackUrl) ? request.ReturnUrl : gateway.CallbackUrl;
        var initiated = await provider.InitiateAsync(ToContext(gateway), new GatewayPaymentRequest(
            PaymentId: payment?.PaymentID ?? attempt.QuizAttemptID,
            Amount: attempt.PrivateReviewFee,
            CurrencyCode: attempt.CurrencyCode ?? "",
            OrderNumber: $"QUIZ-{attempt.QuizAttemptID}",
            ReturnUrl: returnUrl!,
            Description: attempt.Quiz.Title,
            PayerEmail: attempt.Email,
            PayerMobile: attempt.Phone), ct);

        if (!initiated.Success || string.IsNullOrWhiteSpace(initiated.Authority))
        {
            if (payment is not null) payment.Status = (byte)PaymentStatus.Failed;
            await db.SaveChangesAsync(ct);
            return QuizActionResult.Fail(initiated.Error ?? "The payment could not be started.");
        }

        attempt.PaymentGatewayID = gateway.PaymentGatewayID;
        attempt.GatewayAuthority = initiated.Authority;
        attempt.PrivateReviewStatus = (byte)QuizPrivateReviewStatus.AwaitingPayment;
        if (payment is not null) payment.GatewayAuthority = initiated.Authority;
        await db.SaveChangesAsync(ct);
        return QuizActionResult.Success(await BuildViewAsync(db, attempt, ct), initiated.RedirectUrl);
    }

    private async Task CloseAttemptAsync(AppDbContext db, QuizAttempt attempt, QuizSubmitRequest request, bool expired, CancellationToken ct)
    {
        var sitting = await db.QuizAttemptQuestions
            .Where(q => q.QuizAttemptID == attempt.QuizAttemptID)
            .Select(q => q.QuizQuestionID)
            .ToListAsync(ct);
        var questions = await db.QuizQuestions.AsNoTracking()
            .Include(q => q.Options)
            .Where(q => sitting.Contains(q.QuizQuestionID))
            .ToListAsync(ct);

        var gradeQuestions = questions.Select(q => new QuizRules.GradeQuestion(
            q.QuizQuestionID,
            q.Points,
            q.AllowMultiple,
            q.Options.Count > 0,
            q.Options.Where(o => o.IsCorrect).Select(o => o.QuizOptionID).ToHashSet())).ToList();

        var relevant = request.Answers.Where(a => sitting.Contains(a.QuizQuestionID)).ToList();
        var grade = QuizRules.GradeAttempt(gradeQuestions, relevant);

        var bands = await db.QuizScoreBands.AsNoTracking()
            .Include(b => b.Translations)
            .Where(b => b.QuizID == attempt.QuizID)
            .ToListAsync(ct);
        var bandInputs = bands.Select(b => new QuizBandInput
        {
            MinPercent = b.MinPercent,
            MaxPercent = b.MaxPercent,
            Title = Pick(b.Title, b.Translations.Select(t => (t.LanguageCode, t.Title)), attempt.LanguageCode),
            Body = PickOptional(b.Body, b.Translations.Select(t => (t.LanguageCode, t.Body)), attempt.LanguageCode) ?? "",
            SortOrder = b.SortOrder,
        }).ToList();
        var band = QuizRules.MatchBand(bandInputs, grade.Percent);

        foreach (var question in questions)
        {
            var answer = relevant.LastOrDefault(a => a.QuizQuestionID == question.QuizQuestionID);
            var row = new QuizAttemptAnswer
            {
                QuizAttemptID = attempt.QuizAttemptID,
                QuizQuestionID = question.QuizQuestionID,
                TextAnswer = question.AllowText ? Clean(answer?.Text) : null,
                PointsAwarded = grade.PointsByQuestion.TryGetValue(question.QuizQuestionID, out var points) ? points : 0,
            };
            var allowed = question.Options.Select(o => o.QuizOptionID).ToHashSet();
            foreach (var optionId in (answer?.OptionIds ?? new List<int>()).Where(allowed.Contains).Distinct())
                row.SelectedOptions.Add(new QuizAttemptAnswerOption { QuizOptionID = optionId });
            db.QuizAttemptAnswers.Add(row);
        }

        attempt.Score = grade.Score;
        attempt.MaxScore = grade.MaxScore;
        attempt.Percent = grade.Percent;
        attempt.BandTitle = band?.Title;
        attempt.BandBody = band?.Body;
        attempt.SubmittedAt = DateTime.UtcNow;
        attempt.Status = expired ? (byte)QuizAttemptStatus.Expired : (byte)QuizAttemptStatus.Submitted;
        await db.SaveChangesAsync(ct);
    }

    private async Task<QuizAttemptViewDto> BuildViewAsync(AppDbContext db, QuizAttempt attempt, CancellationToken ct)
    {
        var title = await db.Quizzes.Where(q => q.QuizID == attempt.QuizID).Select(q => q.Title).FirstAsync(ct);
        var view = new QuizAttemptViewDto
        {
            Token = attempt.PublicToken.ToString("N"),
            QuizID = attempt.QuizID,
            Title = title,
            ExpiresAtUtc = DateTime.SpecifyKind(attempt.ExpiresAt, DateTimeKind.Utc),
            DurationMinutes = (int)Math.Max(1, (attempt.ExpiresAt - attempt.StartedAt).TotalMinutes),
            Submitted = attempt.Status != (byte)QuizAttemptStatus.InProgress,
        };

        if (!view.Submitted)
        {
            var rows = await db.QuizAttemptQuestions.AsNoTracking()
                .Where(q => q.QuizAttemptID == attempt.QuizAttemptID)
                .OrderBy(q => q.SortOrder)
                .Include(q => q.QuizQuestion).ThenInclude(q => q.Options)
                .Include(q => q.QuizQuestion).ThenInclude(q => q.Images).ThenInclude(i => i.FileRecord)
                .ToListAsync(ct);

            view.Questions = rows.Select(link => link.QuizQuestion).Select(q => new QuizAttemptQuestionDto
            {
                QuizQuestionID = q.QuizQuestionID,
                Prompt = q.Prompt,
                AllowMultiple = q.AllowMultiple,
                AllowText = q.AllowText,
                ImageUrls = q.Images.OrderBy(i => i.SortOrder)
                    .Select(i => i.FileRecord.CNDUrl ?? i.FileRecord.ThumbnailCDN)
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .Select(url => url!)
                    .ToList(),
                Options = q.Options.OrderBy(o => o.SortOrder).Select(o => new QuizAttemptOptionDto
                {
                    QuizOptionID = o.QuizOptionID,
                    Text = o.Text,
                }).ToList(),
            }).ToList();
            return view;
        }

        var quiz = await db.Quizzes.AsNoTracking().FirstAsync(q => q.QuizID == attempt.QuizID, ct);
        var gateways = new List<QuizGatewayChoiceDto>();
        if (quiz.OfferPrivateReview && attempt.PrivateReviewStatus is (byte)QuizPrivateReviewStatus.None or (byte)QuizPrivateReviewStatus.AwaitingPayment)
        {
            var available = await _onlinePayments.GetAvailableAsync(attempt.WebsiteID, ct);
            gateways = available.Select(g => new QuizGatewayChoiceDto
            {
                PaymentGatewayID = g.PaymentGatewayID,
                Name = string.IsNullOrWhiteSpace(g.Name) ? g.ProviderDisplayName : g.Name,
            }).ToList();
        }

        view.Result = new QuizResultDto
        {
            Score = attempt.Score,
            MaxScore = attempt.MaxScore,
            Percent = attempt.Percent,
            BandTitle = attempt.BandTitle,
            BandBody = attempt.BandBody,
            OfferPrivateReview = quiz.OfferPrivateReview,
            PrivateReviewFee = quiz.PrivateReviewFee,
            CurrencyCode = quiz.CurrencyCode,
            PrivateReviewStatus = attempt.PrivateReviewStatus,
            PrivateAnswer = attempt.PrivateReviewStatus == (byte)QuizPrivateReviewStatus.Delivered ? attempt.PrivateAnswer : null,
            ContactName = attempt.ContactName,
            Gateways = gateways,
        };
        return view;
    }

    private static void MarkPaid(QuizAttempt attempt, string? reference)
    {
        attempt.PrivateReviewStatus = (byte)QuizPrivateReviewStatus.PaidAwaitingAnswer;
        attempt.PaidAt = DateTime.UtcNow;
        attempt.GatewayRef = string.IsNullOrWhiteSpace(reference) ? attempt.GatewayRef : reference.Trim();
        if (attempt.Payment is not null)
        {
            attempt.Payment.Status = (byte)PaymentStatus.Paid;
            attempt.Payment.PaidAt = DateTime.UtcNow;
            attempt.Payment.GatewayRefNumber = attempt.GatewayRef;
        }
    }

    private async Task NotifyReviewAsync(QuizAttempt attempt, string message, CancellationToken ct)
    {
        try
        {
            await _notifications.NotifyRoleAsync(
                attempt.WebsiteID,
                new[] { RoleKeys.QuizzesReview },
                AdminNotificationType.QuizPrivateReview,
                attempt.Quiz.Title,
                message,
                $"/content/quizzes/{attempt.QuizID}/attempts",
                attempt.QuizAttemptID,
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not notify admins about quiz attempt {AttemptId}.", attempt.QuizAttemptID);
        }
    }

    private static async Task<int?> FindClientIdAsync(AppDbContext db, QuizAttempt attempt, CancellationToken ct)
    {
        if (attempt.WebsiteClientID is int existing) return existing;
        var email = attempt.Email?.Trim();
        if (!string.IsNullOrWhiteSpace(email))
        {
            var byEmail = await db.WebsiteClients
                .Where(c => c.WebsiteID == attempt.WebsiteID && c.Email == email)
                .Select(c => (int?)c.WebsiteClientID)
                .FirstOrDefaultAsync(ct);
            if (byEmail is not null) return byEmail;
        }

        var phone = attempt.Phone?.Trim();
        if (string.IsNullOrWhiteSpace(phone)) return null;
        return await db.WebsiteClients
            .Where(c => c.WebsiteID == attempt.WebsiteID && c.Cellphone == phone)
            .Select(c => (int?)c.WebsiteClientID)
            .FirstOrDefaultAsync(ct);
    }

    private static async Task<QuizAttempt?> OwnedAttemptAsync(AppDbContext db, int attemptId, int? websiteId, CancellationToken ct)
    {
        var attempt = await db.QuizAttempts.Include(a => a.Payment)
            .FirstOrDefaultAsync(a => a.QuizAttemptID == attemptId, ct);
        if (attempt is null) return null;
        if (websiteId is int id && attempt.WebsiteID != id) return null;
        return attempt;
    }

    private static async Task<string> UniqueCategorySlugAsync(AppDbContext db, QuizCategoryEditorDto model, CancellationToken ct)
    {
        var slug = SlugGenerator.Normalize(string.IsNullOrWhiteSpace(model.Slug) ? model.Title : model.Slug, 160);
        var existing = await db.QuizCategories
            .Where(c => c.WebsiteID == model.WebsiteID && c.QuizCategoryID != model.QuizCategoryID)
            .Select(c => c.Slug)
            .ToListAsync(ct);
        return SlugGenerator.MakeUnique(slug, existing.ToHashSet(StringComparer.OrdinalIgnoreCase));
    }

    private static async Task<string> UniqueQuizSlugAsync(AppDbContext db, QuizEditorDto model, CancellationToken ct)
    {
        var slug = SlugGenerator.Normalize(string.IsNullOrWhiteSpace(model.Slug) ? model.Title : model.Slug, 160);
        var existing = await db.Quizzes
            .Where(q => q.WebsiteID == model.WebsiteID && q.QuizID != model.QuizID)
            .Select(q => q.Slug)
            .ToListAsync(ct);
        return SlugGenerator.MakeUnique(slug, existing.ToHashSet(StringComparer.OrdinalIgnoreCase));
    }

    private static async Task<string?> ValidateFilesAsync(AppDbContext db, QuizEditorDto model, CancellationToken ct)
    {
        var fileIds = model.Questions.SelectMany(q => q.Images).Select(i => i.FileRecordID).Where(id => id > 0).Distinct().ToList();
        if (fileIds.Count == 0) return null;
        var owned = await (
            from file in db.FileRecords
            join storage in db.WebsiteStorageSettings on file.WebsiteStorageSettingsID equals storage.WebsiteStorageSettingsID
            where fileIds.Contains(file.FileRecordID) && storage.WebsiteID == model.WebsiteID
            select file.FileRecordID).ToListAsync(ct);
        return owned.Count == fileIds.Count ? null : "One of the question images is not in this website's media library.";
    }

    private static async Task<string?> ReplaceChildrenAsync(AppDbContext db, Quiz quiz, QuizEditorDto model, CancellationToken ct)
    {
        var usedQuestions = await db.QuizAttemptQuestions
            .Where(x => x.QuizQuestion.QuizID == quiz.QuizID)
            .Select(x => x.QuizQuestionID)
            .ToListAsync(ct);
        var usedOptions = await db.QuizAttemptAnswerOptions
            .Where(x => x.QuizOption.QuizQuestion.QuizID == quiz.QuizID)
            .Select(x => x.QuizOptionID)
            .ToListAsync(ct);

        var incomingQuestions = model.Questions.Select(q => q.QuizQuestionID).ToHashSet();
        foreach (var existing in quiz.Questions.Where(q => q.QuizQuestionID != 0 && !incomingQuestions.Contains(q.QuizQuestionID)).ToList())
        {
            if (usedQuestions.Contains(existing.QuizQuestionID))
                return "A question that someone has already taken cannot be removed. Turn it off instead.";
            db.QuizQuestions.Remove(existing);
        }

        foreach (var input in model.Questions)
        {
            var question = input.QuizQuestionID == 0
                ? null
                : quiz.Questions.FirstOrDefault(q => q.QuizQuestionID == input.QuizQuestionID);
            if (question is null)
            {
                question = new QuizQuestion();
                quiz.Questions.Add(question);
            }

            question.Prompt = input.Prompt.Trim();
            question.Points = input.Points;
            question.AllowMultiple = input.AllowMultiple;
            question.AllowText = input.AllowText;
            question.SortOrder = input.SortOrder;
            question.IsActive = input.IsActive;

            var incomingOptions = input.Options.Select(o => o.QuizOptionID).ToHashSet();
            foreach (var option in question.Options.Where(o => o.QuizOptionID != 0 && !incomingOptions.Contains(o.QuizOptionID)).ToList())
            {
                if (usedOptions.Contains(option.QuizOptionID))
                    return "An option that someone has already chosen cannot be removed.";
                db.QuizOptions.Remove(option);
            }

            var optionOrder = 0;
            foreach (var optionInput in input.Options.Where(o => !string.IsNullOrWhiteSpace(o.Text)))
            {
                var option = optionInput.QuizOptionID == 0
                    ? null
                    : question.Options.FirstOrDefault(o => o.QuizOptionID == optionInput.QuizOptionID);
                if (option is null)
                {
                    option = new QuizOption();
                    question.Options.Add(option);
                }

                option.Text = optionInput.Text.Trim();
                option.IsCorrect = optionInput.IsCorrect;
                option.SortOrder = optionOrder++;
            }

            db.QuizQuestionImages.RemoveRange(question.Images);
            question.Images.Clear();
            var imageOrder = 0;
            foreach (var image in input.Images.Where(i => i.FileRecordID > 0))
            {
                question.Images.Add(new QuizQuestionImage
                {
                    FileRecordID = image.FileRecordID,
                    SortOrder = imageOrder++,
                });
            }
        }

        db.QuizScoreBands.RemoveRange(quiz.ScoreBands);
        quiz.ScoreBands.Clear();
        db.QuizTranslations.RemoveRange(quiz.Translations);
        quiz.Translations.Clear();
        AddBandsAndTranslations(quiz, model);
        return null;
    }

    private static void AddChildren(Quiz quiz, QuizEditorDto model)
    {
        foreach (var input in model.Questions)
        {
            var question = new QuizQuestion
            {
                Prompt = input.Prompt.Trim(),
                Points = input.Points,
                AllowMultiple = input.AllowMultiple,
                AllowText = input.AllowText,
                SortOrder = input.SortOrder,
                IsActive = input.IsActive,
            };
            var optionOrder = 0;
            foreach (var option in input.Options.Where(o => !string.IsNullOrWhiteSpace(o.Text)))
            {
                question.Options.Add(new QuizOption
                {
                    Text = option.Text.Trim(),
                    IsCorrect = option.IsCorrect,
                    SortOrder = optionOrder++,
                });
            }

            var imageOrder = 0;
            foreach (var image in input.Images.Where(i => i.FileRecordID > 0))
            {
                question.Images.Add(new QuizQuestionImage
                {
                    FileRecordID = image.FileRecordID,
                    SortOrder = imageOrder++,
                });
            }

            quiz.Questions.Add(question);
        }

        AddBandsAndTranslations(quiz, model);
    }

    private static void AddBandsAndTranslations(Quiz quiz, QuizEditorDto model)
    {
        foreach (var text in model.Translations.Where(t => !string.IsNullOrWhiteSpace(t.LanguageCode)))
        {
            if (string.IsNullOrWhiteSpace(text.Title) && string.IsNullOrWhiteSpace(text.Body)) continue;
            quiz.Translations.Add(new QuizTranslation
            {
                LanguageCode = text.LanguageCode.Trim().ToLowerInvariant(),
                Title = Clean(text.Title),
                Summary = Clean(text.Body),
            });
        }

        var order = 0;
        foreach (var band in model.Bands)
        {
            var row = new QuizScoreBand
            {
                MinPercent = band.MinPercent,
                MaxPercent = band.MaxPercent,
                Title = band.Title.Trim(),
                Body = band.Body.Trim(),
                SortOrder = order++,
            };
            foreach (var text in band.Translations.Where(t => !string.IsNullOrWhiteSpace(t.LanguageCode)))
            {
                if (string.IsNullOrWhiteSpace(text.Title) && string.IsNullOrWhiteSpace(text.Body)) continue;
                row.Translations.Add(new QuizScoreBandTranslation
                {
                    LanguageCode = text.LanguageCode.Trim().ToLowerInvariant(),
                    Title = Clean(text.Title),
                    Body = Clean(text.Body),
                });
            }

            quiz.ScoreBands.Add(row);
        }
    }

    private static QuizPublicCategoryDto ToPublicCategory(QuizCategory category, string? lang) => new()
    {
        QuizCategoryID = category.QuizCategoryID,
        Title = Pick(category.Title, category.Translations.Select(t => (t.LanguageCode, t.Title)), lang),
        Slug = category.Slug,
        Description = PickOptional(category.Description, category.Translations.Select(t => (t.LanguageCode, t.Description)), lang),
        ShowInPosts = category.ShowInPosts,
        Quizzes = category.Quizzes.Where(q => q.IsPublished).OrderBy(q => q.Title).Select(q =>
        {
            var active = q.Questions.Count(x => x.IsActive);
            return new QuizPublicSummaryDto
            {
                QuizID = q.QuizID,
                Title = Pick(q.Title, q.Translations.Select(t => (t.LanguageCode, t.Title)), lang),
                Summary = PickOptional(q.Summary, q.Translations.Select(t => (t.LanguageCode, t.Summary)), lang),
                DurationMinutes = q.DurationMinutes,
                QuestionCount = q.QuestionMode == (byte)QuizQuestionMode.Random
                    ? Math.Min(q.RandomCount ?? active, active)
                    : active,
                QuestionMode = q.QuestionMode,
                RandomCount = q.RandomCount,
            };
        }).ToList(),
    };

    private static string Pick(string fallback, IEnumerable<(string LanguageCode, string? Text)> rows, string? lang)
    {
        if (!string.IsNullOrWhiteSpace(lang))
        {
            var hit = rows.FirstOrDefault(r =>
                string.Equals(r.LanguageCode, lang, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(r.Text));
            if (!string.IsNullOrWhiteSpace(hit.Text)) return hit.Text!;
        }

        return fallback;
    }

    private static string? PickOptional(string? fallback, IEnumerable<(string LanguageCode, string? Text)> rows, string? lang)
    {
        if (!string.IsNullOrWhiteSpace(lang))
        {
            var hit = rows.FirstOrDefault(r =>
                string.Equals(r.LanguageCode, lang, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(r.Text));
            if (!string.IsNullOrWhiteSpace(hit.Text)) return hit.Text;
        }

        return fallback;
    }

    private static string? Clean(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static string? ValidateContact(QuizPrivateReviewRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName)) return "Enter your name.";
        if (request.FullName.Trim().Length > 120) return "The name is too long.";
        var digits = request.Phone?.Count(char.IsDigit) ?? 0;
        if (digits < 6) return "Enter a phone number.";
        var email = request.Email?.Trim() ?? "";
        if (email.Length > 200 || !email.Contains('@') || !email.Contains('.')) return "Enter an email address.";
        return null;
    }

    private static PaymentGatewayContext ToContext(PaymentGateway row) => new()
    {
        PaymentGatewayID = row.PaymentGatewayID,
        WebsiteID = row.WebsiteID,
        Provider = row.Provider,
        SettingsJson = string.IsNullOrWhiteSpace(row.SettingsJSON) ? "{}" : row.SettingsJSON,
        MerchantId = row.MerchantID,
        ApiKey = row.ApiKey,
        ApiSecret = row.ApiSecret,
        IsSandbox = row.IsSandbox,
        CallbackUrl = row.CallbackUrl,
    };
}
