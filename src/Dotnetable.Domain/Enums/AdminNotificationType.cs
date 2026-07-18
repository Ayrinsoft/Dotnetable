namespace Dotnetable.Domain.Enums;

public enum AdminNotificationType : byte
{
    ContactMessage = 1,
    FormSubmission = 2,
    ClientRegistered = 3,
    NewOrder = 4,
    BankReceipt = 5,
    PaymentReceived = 6,
    WithdrawalRequested = 7,
}
