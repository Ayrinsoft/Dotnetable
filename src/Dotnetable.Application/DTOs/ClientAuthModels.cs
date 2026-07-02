namespace Dotnetable.Application.DTOs;

/// <summary>How a one-time code is delivered to a customer.</summary>
public enum OtpChannel
{
    Email = 0,
    Sms = 1,
}

/// <summary>Self-registration input for a website customer (email <em>or</em> mobile + password).</summary>
public sealed record ClientRegistration(
    int WebsiteId,
    string? GivenName,
    string? Surname,
    string? Email,
    string? CountryCode,
    string? Cellphone,
    string Password);

public enum ClientRegisterResult
{
    /// <summary>A new (or re-issued) activation code was sent.</summary>
    OtpSent = 0,

    /// <summary>An active account already exists for this email/mobile — the customer should sign in.</summary>
    AlreadyRegistered = 1,

    /// <summary>Neither a valid email nor a valid mobile was supplied.</summary>
    InvalidInput = 2,

    /// <summary>The delivery channel (SMTP / SMS gateway) is not configured, so no code could be sent.</summary>
    DeliveryNotConfigured = 3,
}

/// <summary>Outcome of a registration attempt, including which channel the activation code went to.</summary>
public sealed record ClientRegisterResponse(ClientRegisterResult Result, OtpChannel Channel, string Identifier);

public enum ClientVerifyResult
{
    Success = 0,
    InvalidCode = 1,
    NotFound = 2,
    AlreadyActive = 3,
}

public enum ClientResendResult
{
    OtpSent = 0,
    NotFound = 1,
    AlreadyActive = 2,
    DeliveryNotConfigured = 3,
}

public enum ClientLoginStatus
{
    Success = 0,
    InvalidCredentials = 1,
    /// <summary>Credentials are correct but the account has not completed OTP activation yet.</summary>
    NotActivated = 2,
}

public enum ClientResetRequestResult
{
    OtpSent = 0,
    NotFound = 1,
    DeliveryNotConfigured = 2,
}

public enum ClientResetResult
{
    Success = 0,
    InvalidCode = 1,
    NotFound = 2,
}
