namespace Dotnetable.Application.DTOs;

public sealed class QuizTextDto
{
    public string LanguageCode { get; set; } = "";
    public string? Title { get; set; }
    public string? Body { get; set; }
}

public sealed class QuizCategoryEditorDto
{
    public int QuizCategoryID { get; set; }
    public int WebsiteID { get; set; }
    public int? ParentCategoryID { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool ShowInPosts { get; set; }
    public List<QuizTextDto> Translations { get; set; } = new();
}

public sealed class QuizCategoryListItemDto
{
    public int QuizCategoryID { get; set; }
    public int WebsiteID { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public bool IsActive { get; set; }
    public bool ShowInPosts { get; set; }
    public int QuizCount { get; set; }
    public int SortOrder { get; set; }
}

public sealed class QuizOptionInput
{
    public int QuizOptionID { get; set; }
    public string Text { get; set; } = "";
    public bool IsCorrect { get; set; }
    public int SortOrder { get; set; }
}

public sealed class QuizImageInput
{
    public int FileRecordID { get; set; }
    public string? Url { get; set; }
    public int SortOrder { get; set; }
}

public sealed class QuizQuestionInput
{
    public int QuizQuestionID { get; set; }
    public string Prompt { get; set; } = "";
    public decimal Points { get; set; } = 1;
    public bool AllowMultiple { get; set; }
    public bool AllowText { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public List<QuizOptionInput> Options { get; set; } = new();
    public List<QuizImageInput> Images { get; set; } = new();
}

public sealed class QuizBandInput
{
    public int QuizScoreBandID { get; set; }
    public decimal MinPercent { get; set; }
    public decimal MaxPercent { get; set; } = 100;
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public int SortOrder { get; set; }
    public List<QuizTextDto> Translations { get; set; } = new();
}

public sealed class QuizEditorDto
{
    public int QuizID { get; set; }
    public int WebsiteID { get; set; }
    public int QuizCategoryID { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Summary { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public byte QuestionMode { get; set; }
    public int? RandomCount { get; set; } = 50;
    public bool IsPublished { get; set; }
    public bool OfferPrivateReview { get; set; }
    public decimal PrivateReviewFee { get; set; }
    public string? CurrencyCode { get; set; }
    public List<QuizTextDto> Translations { get; set; } = new();
    public List<QuizBandInput> Bands { get; set; } = new();
    public List<QuizQuestionInput> Questions { get; set; } = new();
}

public sealed class QuizListItemDto
{
    public int QuizID { get; set; }
    public int WebsiteID { get; set; }
    public int QuizCategoryID { get; set; }
    public string CategoryTitle { get; set; } = "";
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public int DurationMinutes { get; set; }
    public byte QuestionMode { get; set; }
    public int? RandomCount { get; set; }
    public int QuestionCount { get; set; }
    public bool IsPublished { get; set; }
    public bool OfferPrivateReview { get; set; }
    public int AttemptCount { get; set; }
}

public sealed class QuizPublicSummaryDto
{
    public int QuizID { get; set; }
    public string Title { get; set; } = "";
    public string? Summary { get; set; }
    public int DurationMinutes { get; set; }
    public int QuestionCount { get; set; }
    public byte QuestionMode { get; set; }
    public int? RandomCount { get; set; }
}

public sealed class QuizPublicCategoryDto
{
    public int QuizCategoryID { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Description { get; set; }
    public bool ShowInPosts { get; set; }
    public List<QuizPublicSummaryDto> Quizzes { get; set; } = new();
}

public sealed class QuizPublicDetailDto
{
    public int QuizID { get; set; }
    public string Title { get; set; } = "";
    public string? Summary { get; set; }
    public string CategoryTitle { get; set; } = "";
    public int CategoryId { get; set; }
    public int DurationMinutes { get; set; }
    public int QuestionCount { get; set; }
    public byte QuestionMode { get; set; }
    public int? RandomCount { get; set; }
    public bool OfferPrivateReview { get; set; }
    public decimal PrivateReviewFee { get; set; }
    public string? CurrencyCode { get; set; }
}

public sealed class QuizAttemptOptionDto
{
    public int QuizOptionID { get; set; }
    public string Text { get; set; } = "";
}

public sealed class QuizAttemptQuestionDto
{
    public int QuizQuestionID { get; set; }
    public string Prompt { get; set; } = "";
    public bool AllowMultiple { get; set; }
    public bool AllowText { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public List<QuizAttemptOptionDto> Options { get; set; } = new();
}

public sealed class QuizGatewayChoiceDto
{
    public int PaymentGatewayID { get; set; }
    public string Name { get; set; } = "";
}

public sealed class QuizResultDto
{
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public decimal Percent { get; set; }
    public string? BandTitle { get; set; }
    public string? BandBody { get; set; }
    public bool OfferPrivateReview { get; set; }
    public decimal PrivateReviewFee { get; set; }
    public string? CurrencyCode { get; set; }
    public byte PrivateReviewStatus { get; set; }
    public string? PrivateAnswer { get; set; }
    public string? ContactName { get; set; }
    public List<QuizGatewayChoiceDto> Gateways { get; set; } = new();
}

public sealed class QuizAttemptViewDto
{
    public string Token { get; set; } = "";
    public int QuizID { get; set; }
    public string Title { get; set; } = "";
    public DateTime ExpiresAtUtc { get; set; }
    public int DurationMinutes { get; set; }
    public bool Submitted { get; set; }
    public List<QuizAttemptQuestionDto> Questions { get; set; } = new();
    public QuizResultDto? Result { get; set; }
}

public sealed class QuizAnswerInput
{
    public int QuizQuestionID { get; set; }
    public List<int> OptionIds { get; set; } = new();
    public string? Text { get; set; }
}

public sealed class QuizSubmitRequest
{
    public List<QuizAnswerInput> Answers { get; set; } = new();
}

public sealed class QuizPrivateReviewRequest
{
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public bool PayOnline { get; set; }
    public int? GatewayId { get; set; }
    public string? ReturnUrl { get; set; }
}

public sealed class QuizActionResult
{
    public bool Ok { get; init; }
    public string? Message { get; init; }
    public string? RedirectUrl { get; init; }
    public QuizAttemptViewDto? Attempt { get; init; }

    public static QuizActionResult Success(QuizAttemptViewDto? attempt = null, string? redirectUrl = null, string? message = null) =>
        new() { Ok = true, Attempt = attempt, RedirectUrl = redirectUrl, Message = message };

    public static QuizActionResult Fail(string message) => new() { Ok = false, Message = message };
}

public sealed class QuizAttemptAdminAnswerDto
{
    public string Prompt { get; set; } = "";
    public string? TextAnswer { get; set; }
    public decimal PointsAwarded { get; set; }
    public decimal Points { get; set; }
    public List<string> Selected { get; set; } = new();
    public List<string> Correct { get; set; } = new();
}

public sealed class QuizAttemptAdminDto
{
    public int QuizAttemptID { get; set; }
    public string Token { get; set; } = "";
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public decimal Percent { get; set; }
    public string? BandTitle { get; set; }
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public byte PrivateReviewStatus { get; set; }
    public decimal PrivateReviewFee { get; set; }
    public string? CurrencyCode { get; set; }
    public string? PrivateAnswer { get; set; }
    public List<QuizAttemptAdminAnswerDto> Answers { get; set; } = new();
}
