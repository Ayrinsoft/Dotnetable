namespace Dotnetable.Domain.Enums;

/// <summary>How a customer accessed a digital entitlement (stored in DigitalAccessLog.AccessType).</summary>
public enum DigitalAccessType : byte
{
    /// <summary>Opened library detail / order digital section.</summary>
    View = 1,

    /// <summary>Downloaded the digital file.</summary>
    Download = 2,

    /// <summary>Viewed license / delivery note text.</summary>
    ViewCode = 3,

    /// <summary>Opened / copied the service URL.</summary>
    ViewService = 4,
}
