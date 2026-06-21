namespace Dotnetable.Application;

public static class AppConstants
{
    /// <summary>
    /// The master website. WebsiteID 1 is always created first during setup; its members have
    /// access to every other website. Any other member can only see its own website's data.
    /// </summary>
    public const int MasterWebsiteId = 1;
}
