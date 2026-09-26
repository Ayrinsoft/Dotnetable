using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dotnetable.Admin.Components.Pages.Content.Posts;

/// <summary>One language block inside a post import file. Root and each translation share this shape.</summary>
public sealed class PostImportLocale
{
    public string? Language { get; set; }
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Summary { get; set; }
    public string? Excerpt { get; set; }

    [JsonPropertyName("meta_title")]
    public string? MetaTitle { get; set; }

    [JsonPropertyName("meta_description")]
    public string? MetaDescription { get; set; }

    public string? Keywords { get; set; }

    [JsonPropertyName("meta_keywords")]
    public string? MetaKeywords { get; set; }

    [JsonPropertyName("content_html")]
    public string? ContentHtml { get; set; }

    public string? Content { get; set; }

    public string ExcerptText => First(Summary, Excerpt);
    public string BodyHtml => First(ContentHtml, Content);
    public string KeywordsText => First(Keywords, MetaKeywords);

    private static string First(string? a, string? b) =>
        !string.IsNullOrWhiteSpace(a) ? a.Trim() : (b ?? "").Trim();
}

public sealed class PostImportDocument
{
    public string? Language { get; set; }
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Summary { get; set; }
    public string? Excerpt { get; set; }

    [JsonPropertyName("meta_title")]
    public string? MetaTitle { get; set; }

    [JsonPropertyName("meta_description")]
    public string? MetaDescription { get; set; }

    public string? Keywords { get; set; }

    [JsonPropertyName("meta_keywords")]
    public string? MetaKeywords { get; set; }

    [JsonPropertyName("content_html")]
    public string? ContentHtml { get; set; }

    public string? Content { get; set; }

    public Dictionary<string, PostImportLocale>? Translations { get; set; }

    public PostImportLocale AsLocale() => new()
    {
        Language = Language,
        Title = Title,
        Slug = Slug,
        Summary = Summary,
        Excerpt = Excerpt,
        MetaTitle = MetaTitle,
        MetaDescription = MetaDescription,
        Keywords = Keywords,
        MetaKeywords = MetaKeywords,
        ContentHtml = ContentHtml,
        Content = Content,
    };
}

/// <summary>Canonical file written for agents and read back by <see cref="PostImportParser"/>.</summary>
public sealed class PostImportFile
{
    public string? Language { get; set; }
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Summary { get; set; }

    [JsonPropertyName("meta_title")]
    public string? MetaTitle { get; set; }

    [JsonPropertyName("meta_description")]
    public string? MetaDescription { get; set; }

    public string? Keywords { get; set; }

    [JsonPropertyName("content_html")]
    public string? ContentHtml { get; set; }

    public Dictionary<string, PostImportFile>? Translations { get; set; }
}

public static class PostImportWriter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = global::System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static string Write(string language, PostImportLocale main, IEnumerable<KeyValuePair<string, PostImportLocale>> translations)
    {
        var file = From(language, main);
        var others = new Dictionary<string, PostImportFile>(StringComparer.OrdinalIgnoreCase);
        foreach (var (code, locale) in translations)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Equals(language, StringComparison.OrdinalIgnoreCase)) continue;
            if (!HasText(locale)) continue;
            others[code.Trim()] = From(code.Trim(), locale);
        }
        if (others.Count > 0) file.Translations = others;
        return JsonSerializer.Serialize(file, Options);
    }

    private static PostImportFile From(string language, PostImportLocale locale) => new()
    {
        Language = language,
        Title = locale.Title,
        Slug = locale.Slug,
        Summary = locale.ExcerptText,
        MetaTitle = locale.MetaTitle,
        MetaDescription = locale.MetaDescription,
        Keywords = locale.KeywordsText,
        ContentHtml = locale.BodyHtml,
    };

    private static bool HasText(PostImportLocale locale) =>
        !string.IsNullOrWhiteSpace(locale.Title)
        || !string.IsNullOrWhiteSpace(locale.Slug)
        || !string.IsNullOrWhiteSpace(locale.ExcerptText)
        || !string.IsNullOrWhiteSpace(locale.MetaTitle)
        || !string.IsNullOrWhiteSpace(locale.MetaDescription)
        || !string.IsNullOrWhiteSpace(locale.KeywordsText)
        || !string.IsNullOrWhiteSpace(locale.BodyHtml);
}

public static class PostImportParser
{
    public const long MaxBytes = 2 * 1024 * 1024;

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static PostImportDocument Parse(string json)
    {
        var doc = JsonSerializer.Deserialize<PostImportDocument>(json, Options)
            ?? throw new InvalidOperationException("The file is empty.");
        if (string.IsNullOrWhiteSpace(doc.Title) && string.IsNullOrWhiteSpace(doc.ContentHtml) && string.IsNullOrWhiteSpace(doc.Content))
            throw new InvalidOperationException("The file has no title or content.");
        return doc;
    }
}

/// <summary>Shape agents should emit. Shown on the new-post page; nothing here is stored.</summary>
public static class PostImportSample
{
    public const string Json = """
        {
          "language": "fa",
          "title": "عنوان پست",
          "slug": "post-slug",
          "summary": "خلاصهٔ کوتاه برای فهرست و توضیح متا.",
          "meta_title": "عنوان سئو",
          "meta_description": "توضیح نتیجهٔ جستجو، حدود ۱۵۰ نویسه.",
          "keywords": "کلمه۱, کلمه۲, کلمه۳",
          "content_html": "<p>متن اصلی با HTML.</p><h2>سرفصل</h2><p>ادامه.</p>",
          "translations": {
            "en": {
              "language": "en",
              "title": "Post title",
              "slug": "post-slug-en",
              "summary": "Short excerpt.",
              "meta_title": "SEO title",
              "meta_description": "Search snippet.",
              "keywords": "keyword1, keyword2",
              "content_html": "<p>Body HTML.</p>"
            }
          }
        }
        """;
}
