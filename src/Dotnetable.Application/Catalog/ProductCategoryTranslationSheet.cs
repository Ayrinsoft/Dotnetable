namespace Dotnetable.Application.Catalog;

/// <summary>Column contract for the product-category translation workbook.</summary>
public static class ProductCategoryTranslationSheet
{
    public const string SheetName = "Categories";

    public const string ProductCategoryId = "ProductCategoryID";
    public const string GoogleProductCategoryId = "GoogleProductCategoryId";
    public const string LanguageCode = "LanguageCode";
    public const string EnglishPath = "EnglishPath";
    public const string EnglishName = "EnglishName";
    public const string TranslatedName = "TranslatedName";
    public const string TranslatedSlug = "TranslatedSlug";

    public static readonly string[] Headers =
    [
        ProductCategoryId,
        GoogleProductCategoryId,
        LanguageCode,
        EnglishPath,
        EnglishName,
        TranslatedName,
        TranslatedSlug,
    ];
}
