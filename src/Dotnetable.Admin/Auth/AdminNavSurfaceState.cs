using Dotnetable.Domain.Enums;

namespace Dotnetable.Admin.Auth;

/// <summary>
/// Cached nav surface for the current circuit. Built from the logged-in member's website
/// <see cref="WebsiteType"/> (Website form field) and <see cref="AdminUiMode"/>.
/// </summary>
public sealed class AdminNavSurfaceState
{
    private IReadOnlySet<AdminNavArea> _areas = new HashSet<AdminNavArea>(Enum.GetValues<AdminNavArea>());

    public IReadOnlySet<AdminNavArea> Areas => _areas;

    /// <summary>Exact type from Websites.WebsiteType for the login website (null on master / unknown).</summary>
    public WebsiteType? WebsiteType { get; private set; }

    public WebsiteCategory? Category => WebsiteType?.GetCategory();

    public bool IsVendorMember { get; private set; }

    public int? VendorId { get; private set; }

    public int? WebsiteId { get; private set; }

    public event Action? Changed;

    public void Rebuild(AdminUiMode mode, WebsiteType? websiteType, bool isMaster, int? vendorId, int? websiteId = null)
    {
        WebsiteType = websiteType;
        WebsiteId = websiteId;
        VendorId = vendorId is > 0 ? vendorId : null;
        IsVendorMember = VendorId is not null;
        _areas = AdminNavSurface.Resolve(mode, websiteType, isMaster, IsVendorMember);
        Changed?.Invoke();
    }

    public bool Shows(AdminNavArea area) => AdminNavSurface.Shows(_areas, area);

    public bool ShowsAny(params AdminNavArea[] areas) => AdminNavSurface.ShowsAny(_areas, areas);
}
