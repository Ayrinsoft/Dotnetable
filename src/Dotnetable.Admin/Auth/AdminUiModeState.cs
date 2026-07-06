namespace Dotnetable.Admin.Auth;

/// <summary>
/// Scoped, per-circuit override of the signed-in member's <see cref="AdminUiMode"/>. Starts at the
/// member's stored mode (from the AdminUiMode claim) and can be overridden for the current
/// session/browser only via the header dropdown — it never writes back to Member.AdminUIMode.
/// </summary>
public sealed class AdminUiModeState
{
    private bool _initialized;

    public AdminUiMode Current { get; private set; } = AdminUiMode.General;

    public event Action? Changed;

    public void Initialize(AdminUiMode claimMode)
    {
        if (_initialized) return;
        _initialized = true;
        Current = claimMode;
    }

    public void Set(AdminUiMode mode)
    {
        if (Current == mode) return;
        Current = mode;
        Changed?.Invoke();
    }
}
