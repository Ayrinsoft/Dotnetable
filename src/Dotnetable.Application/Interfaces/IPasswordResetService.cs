namespace Dotnetable.Application.Interfaces;

/// <summary>Outcome of a forgot-password request, kept deliberately vague to avoid leaking which accounts exist.</summary>
public enum PasswordResetRequestResult
{
    /// <summary>A reset email was sent (or would have been — see <see cref="EmailNotConfigured"/>).</summary>
    Sent = 0,

    /// <summary>No matching active member; nothing was sent (caller should still show a generic message).</summary>
    MemberNotFound = 1,

    /// <summary>Matched a member but the SMTP settings are missing, so no email could be sent.</summary>
    EmailNotConfigured = 2,
}

public interface IPasswordResetService
{
    /// <summary>
    /// Looks up an active member by email or username and, if found, creates a reset key and emails
    /// a reset link built from <paramref name="resetUrlBuilder"/> (which receives the raw key).
    /// </summary>
    Task<PasswordResetRequestResult> RequestResetAsync(
        string emailOrUsername, Func<string, string> resetUrlBuilder, CancellationToken ct = default);

    /// <summary>True when the key exists and has not expired.</summary>
    Task<bool> IsKeyValidAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Applies a new password for the member that owns a valid key and invalidates outstanding keys.
    /// Returns false when the key is unknown or expired.
    /// </summary>
    Task<bool> ResetPasswordAsync(string key, string newPassword, CancellationToken ct = default);
}
