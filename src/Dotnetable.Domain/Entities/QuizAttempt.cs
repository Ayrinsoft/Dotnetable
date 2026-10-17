namespace Dotnetable.Domain.Entities;

/// <summary>One sitting. <see cref="PublicToken"/> is the only handle the storefront keeps.</summary>
public class QuizAttempt
{
    public int QuizAttemptID { get; set; }

    public int QuizID { get; set; }

    public int WebsiteID { get; set; }

    public int? WebsiteClientID { get; set; }

    public Guid PublicToken { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public byte Status { get; set; }

    public decimal Score { get; set; }

    public decimal MaxScore { get; set; }

    public decimal Percent { get; set; }

    public string? BandTitle { get; set; }

    public string? BandBody { get; set; }

    public bool WantsPrivateReview { get; set; }

    public string? ContactName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public byte PrivateReviewStatus { get; set; }

    public decimal PrivateReviewFee { get; set; }

    public string? CurrencyCode { get; set; }

    public int? PaymentID { get; set; }

    public int? PaymentGatewayID { get; set; }

    public string? GatewayAuthority { get; set; }

    public string? GatewayRef { get; set; }

    public DateTime? PaidAt { get; set; }

    public string? PrivateAnswer { get; set; }

    public DateTime? PrivateAnswerAt { get; set; }

    public string? LanguageCode { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual WebsiteClient? WebsiteClient { get; set; }

    public virtual Payment? Payment { get; set; }

    public virtual PaymentGateway? PaymentGateway { get; set; }

    public virtual ICollection<QuizAttemptQuestion> Questions { get; set; } = new List<QuizAttemptQuestion>();

    public virtual ICollection<QuizAttemptAnswer> Answers { get; set; } = new List<QuizAttemptAnswer>();
}
