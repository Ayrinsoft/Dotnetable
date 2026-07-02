using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Customer (website client) authentication: self-registration with OTP activation, resend,
/// credential login, and OTP-based password reset. Customers sign in with their email or mobile
/// number; they carry no policy — only a loyalty level.
/// </summary>
public interface IWebsiteClientAuthService
{
    /// <summary>
    /// Registers a new customer as inactive and sends an activation OTP to their email or mobile.
    /// If an inactive account already exists for the identifier, no new row is created — a fresh
    /// code is issued instead. An already-active account is reported so the caller can prompt sign-in.
    /// </summary>
    Task<ClientRegisterResponse> RegisterAsync(ClientRegistration registration, CancellationToken ct = default);

    /// <summary>Validates an activation code and, on success, activates the account and returns it (for token issuance).</summary>
    Task<(ClientVerifyResult Result, WebsiteClient? Client)> VerifyOtpAsync(
        int websiteId, string identifier, string code, CancellationToken ct = default);

    /// <summary>Re-issues an activation code for an existing inactive account (no new record).</summary>
    Task<ClientResendResult> ResendOtpAsync(int websiteId, string identifier, CancellationToken ct = default);

    /// <summary>Verifies credentials. Returns the account only when it is active; otherwise the status says why.</summary>
    Task<(ClientLoginStatus Status, WebsiteClient? Client)> ValidateCredentialsAsync(
        int websiteId, string identifier, string password, CancellationToken ct = default);

    /// <summary>Sends a password-reset OTP to the account's email or mobile.</summary>
    Task<ClientResetRequestResult> RequestPasswordResetAsync(int websiteId, string identifier, CancellationToken ct = default);

    /// <summary>Applies a new password when a valid reset code is presented.</summary>
    Task<ClientResetResult> ResetPasswordAsync(
        int websiteId, string identifier, string code, string newPassword, CancellationToken ct = default);
}
