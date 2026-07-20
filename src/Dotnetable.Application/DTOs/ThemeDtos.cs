namespace Dotnetable.Application.DTOs;

/// <summary>Metadata file (theme.json) inside a theme zip package.</summary>
public sealed class ThemeManifest
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Version { get; set; }
    public string? Author { get; set; }
    public string? Description { get; set; }
}

/// <summary>Admin / public listing of an installed or built-in theme.</summary>
public sealed class ThemePackageDto
{
    public int? WebsiteThemeID { get; set; }
    public int WebsiteID { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Version { get; set; }
    public string? Author { get; set; }
    public string? Description { get; set; }
    public bool HasScreenshot { get; set; }
    public bool IsActive { get; set; }
    public bool IsBuiltin { get; set; }
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Relative view root used by the public site view engine, e.g. "Default" or "12/ocean".
    /// </summary>
    public string ViewRoot { get; set; } = string.Empty;
}

/// <summary>Active theme payload for public front-ends (MVC Web host).</summary>
public sealed class ActiveThemeDto
{
    public string Slug { get; set; } = "Default";
    public string Name { get; set; } = "Default";
    public string? Version { get; set; }
    public string ViewRoot { get; set; } = "Default";
}
