namespace Dotnetable.Domain.Enums;

/// <summary>
/// High-level grouping of <see cref="WebsiteType"/> values, used to organize the type picker
/// and to drive behavior that applies to a whole family of site types (e.g. which dashboard
/// layout to show) instead of a single one.
/// </summary>
public enum WebsiteCategory : byte
{
    /// <summary>Brochure-style sites: corporate, personal, landing pages, resumes.</summary>
    Informational = 0,

    /// <summary>Sites built to sell or list something: stores, catalogs, auctions, real estate, restaurants.</summary>
    Commerce = 1,

    /// <summary>Sites organized around publishing content: galleries, portfolios, blogs, news/magazines.</summary>
    Content = 2,

    /// <summary>Educational and professional service sites: courses, memberships, bookings/appointments.</summary>
    Educational = 3,
}
