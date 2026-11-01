namespace Dotnetable.Application.DTOs;

public sealed class WebsiteCategoryVisibilityDto
{
    public bool LimitToSelection { get; init; }
    public IReadOnlyList<int> CategoryIds { get; init; } = Array.Empty<int>();
}

public sealed class ProductCategoryPickDto
{
    public int ProductCategoryID { get; set; }
    public int? ParentCategoryID { get; set; }
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public bool HasChildren { get; set; }
    public bool IsActive { get; set; }
}

public sealed class GoogleTaxonomySyncResult
{
    public int Parsed { get; set; }
    public int Added { get; set; }
    public int Updated { get; set; }
    public int MarkedAbsent { get; set; }
    public int NotYetImported { get; set; }
}

public sealed class GoogleTaxonomyDiffItem
{
    public int GoogleId { get; set; }
    public string Name { get; set; } = "";
    public string FullPath { get; set; } = "";
    public byte Depth { get; set; }
    public bool AbsentFromLatest { get; set; }
}

public sealed class GoogleTaxonomyImportResult
{
    public int Created { get; set; }
    public int AlreadyPresent { get; set; }
    public int Skipped { get; set; }
}

public sealed class ProductCategoryTranslationImportResult
{
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = new();
}

public sealed class WebsiteContentCopyResult
{
    public int PostTypes { get; set; }
    public int Categories { get; set; }
    public int Tags { get; set; }
    public int Posts { get; set; }
    public int Pages { get; set; }
    public int Brands { get; set; }
    public int ProductCategories { get; set; }
    public int Products { get; set; }
    public string Note { get; set; } = "";
}
