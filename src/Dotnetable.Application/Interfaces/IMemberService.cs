using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface IMemberService
{
    Task<Member?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Server-side paged/sorted/searched members. <paramref name="websiteId"/> null = all websites (master only).</summary>
    Task<PagedResult<Member>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);

    /// <summary>Validates credentials and returns the member with its Policy and Roles loaded, or null.</summary>
    [Obsolete("Use ValidateSignInAsync, which also applies lockout and reports why a sign-in failed.")]
    Task<Member?> ValidateCredentialsAsync(string username, string password, CancellationToken ct = default);

    /// <summary>
    /// Validates credentials, applies the failed-attempt lockout, and says whether a second factor is
    /// still outstanding. An admin member can read every order and every customer address in the shop,
    /// so unlimited password guessing against this endpoint is the single highest-value target in the
    /// product.
    /// </summary>
    Task<MemberSignInResult> ValidateSignInAsync(string username, string password, CancellationToken ct = default);

    /// <summary>Verifies a TOTP code (or a recovery code) for a member that passed the password step.</summary>
    Task<MemberSignInResult> VerifyTwoFactorAsync(int memberId, string code, CancellationToken ct = default);

    /// <summary>
    /// Starts enrolment: returns a fresh secret and its <c>otpauth://</c> URI without saving anything.
    /// The secret is only persisted once <see cref="ConfirmTwoFactorAsync"/> proves the member's app
    /// produces matching codes — otherwise a mistyped setup would lock them out permanently.
    /// </summary>
    TwoFactorEnrolment BeginTwoFactorEnrolment(Member member, string issuer);

    /// <summary>
    /// Completes enrolment when <paramref name="code"/> matches <paramref name="secret"/>. Returns the
    /// one-time recovery codes, which are shown to the member exactly once and stored only as hashes.
    /// </summary>
    Task<IReadOnlyList<string>?> ConfirmTwoFactorAsync(int memberId, string secret, string code, CancellationToken ct = default);

    /// <summary>Turns the second factor off and discards the secret and recovery codes.</summary>
    Task DisableTwoFactorAsync(int memberId, CancellationToken ct = default);

    /// <summary>WebsiteID of the member with this username (any status), or null if no such member — used to attribute failed logins.</summary>
    Task<int?> GetWebsiteIdByUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>True when a member already exists with this username or email (used to reject duplicate registrations).</summary>
    Task<bool> ExistsAsync(string username, string email, CancellationToken ct = default);

    Task<IEnumerable<Member>> GetByWebsiteAsync(int websiteId, CancellationToken ct = default);

    /// <summary>All members. Only meaningful for master-website administrators.</summary>
    Task<IEnumerable<Member>> GetAllAsync(CancellationToken ct = default);

    Task SetActiveAsync(int id, bool active, CancellationToken ct = default);
    Task<Member> CreateAsync(Member member, string plainPassword, CancellationToken ct = default);
    Task UpdateAsync(Member member, CancellationToken ct = default);
    Task ChangePasswordAsync(int memberId, string newPassword, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Permission keys (Role.RoleKey) granted to the member through its policy.</summary>
    Task<IReadOnlyList<string>> GetRoleKeysAsync(int memberId, CancellationToken ct = default);
}

/// <summary>Why a member sign-in attempt ended the way it did.</summary>
public enum MemberSignInStatus
{
    /// <summary>Fully authenticated; sign the member in.</summary>
    Success = 0,

    /// <summary>Unknown username, wrong password, or a deactivated account.</summary>
    InvalidCredentials = 1,

    /// <summary>Too many consecutive failures; refuse until the lockout window elapses.</summary>
    LockedOut = 2,

    /// <summary>Password accepted, but the member has a second factor enrolled and has not yet supplied it.</summary>
    TwoFactorRequired = 3,

    /// <summary>The submitted TOTP or recovery code did not match.</summary>
    InvalidTwoFactorCode = 4,
}

/// <summary>
/// Outcome of a sign-in step. <paramref name="Member"/> is populated for
/// <see cref="MemberSignInStatus.Success"/> (sign it in) and for
/// <see cref="MemberSignInStatus.TwoFactorRequired"/> (so the caller knows who to challenge) — and
/// for nothing else.
/// </summary>
public sealed record MemberSignInResult(MemberSignInStatus Status, Member? Member = null, DateTime? LockoutEndUtc = null)
{
    public static readonly MemberSignInResult Invalid = new(MemberSignInStatus.InvalidCredentials);
}

/// <summary>A pending TOTP enrolment: the secret to confirm and the URI/QR payload to display.</summary>
public sealed record TwoFactorEnrolment(string Secret, string ProvisioningUri);
