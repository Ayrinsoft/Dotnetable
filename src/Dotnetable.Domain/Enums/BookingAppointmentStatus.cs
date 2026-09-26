namespace Dotnetable.Domain.Enums;

/// <summary>Lifecycle of one reserved window on a booking resource.</summary>
public enum BookingAppointmentStatus : byte
{
    /// <summary>Slot is held while the customer pays. Released when the hold window passes.</summary>
    Hold = 1,

    /// <summary>Bank receipt is in the verification queue. The slot stays taken.</summary>
    AwaitingVerification = 2,

    /// <summary>Paid, or payment was not required.</summary>
    Confirmed = 3,

    Cancelled = 4,
}
