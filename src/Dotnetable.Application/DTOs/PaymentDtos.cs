namespace Dotnetable.Application.DTOs;

/// <summary>A paid payment that still has room for a refund (admin bank-refund picker).</summary>
public sealed class RefundablePaymentDto
{
    public int PaymentID { get; set; }
    public int? OrderID { get; set; }
    public string? OrderNumber { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}
