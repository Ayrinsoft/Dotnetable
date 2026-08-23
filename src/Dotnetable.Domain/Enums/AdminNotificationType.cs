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
    SupportTicket = 8,
    /// <summary>Warehouse document submitted / ready to pick / posted.</summary>
    WarehouseDocument = 9,
    /// <summary>Payroll run awaiting action.</summary>
    PayrollPending = 10,
    /// <summary>Tax period closed or ready to file.</summary>
    TaxPeriod = 11,
    /// <summary>A staff task was assigned to this member.</summary>
    StaffTask = 12,
}
