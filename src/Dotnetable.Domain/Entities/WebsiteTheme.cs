using System;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// An installed front-end theme package for a website (WordPress-style). Physical files live under
/// the configured themes root as <c>{websiteId}/{slug}/</c>; the built-in "Default" theme is virtual
/// (no package folder under a website id — it ships with Dotnetable.Web).
/// </summary>
public partial class WebsiteTheme
{
    public int WebsiteThemeID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>Folder-safe identifier (e.g. ocean). Unique per website.</summary>
    public string Slug { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Version { get; set; }

    public string? Author { get; set; }

    public string? Description { get; set; }

    /// <summary>True when a screenshot.png (or .jpg) exists in the theme package root.</summary>
    public bool HasScreenshot { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
}
