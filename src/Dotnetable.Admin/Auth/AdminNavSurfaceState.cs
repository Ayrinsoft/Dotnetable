using Dotnetable.Domain.Enums;

namespace Dotnetable.Admin.Auth;

/// <summary>
/// Cached nav surface for the current circuit. Rebuilt when UI mode changes or auth context is known.
/// </summary>
public sealed class AdminNavSurfaceState
{
    private IReadOnlySet<AdminNavArea> _areas = new HashSet<AdminNavArea>(Enum.GetValues<AdminNavArea>());

    public IReadOnlySet<AdminNavArea> Areas => _areas;

    public WebsiteCategory? Category { get; private set; }

    public bool IsVendorMember { get; private set; }

    public int? VendorId { get; private set; }

    public event Action? Changed;

    public void Rebuild(AdminUiMode mode, WebsiteCategory? category, bool isMaster, int? vendorId)
    {
        Category = category;
        VendorId = vendorId is > 0 ? vendorId : null;
        IsVendorMember = VendorId is not null;
        _areas = AdminNavSurface.Resolve(mode, category, isMaster, IsVendorMember);
        Changed?.Invoke();
    }

    public bool Shows(AdminNavArea area) => AdminNavSurface.Shows(_areas, area);

    public bool ShowsAny(params AdminNavArea[] areas) => AdminNavSurface.ShowsAny(_areas, areas);
}
