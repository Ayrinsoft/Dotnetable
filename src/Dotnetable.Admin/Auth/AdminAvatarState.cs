namespace Dotnetable.Admin.Auth;

/// <summary>
/// Scoped, per-circuit copy of the signed-in member's avatar URL, so the header avatar updates as
/// soon as the member changes it on "My account" without a page reload.
/// </summary>
public sealed class AdminAvatarState
{
    public string? Url { get; private set; }

    public event Action? Changed;

    public void Set(string? url)
    {
        if (Url == url) return;
        Url = url;
        Changed?.Invoke();
    }
}
