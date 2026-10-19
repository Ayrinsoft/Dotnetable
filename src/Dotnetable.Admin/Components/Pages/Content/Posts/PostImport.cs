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

    public PostImportLocation? Location { get; set; }

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

    public PostImportLocation? Location { get; set; }

    public Dictionary<string, PostImportFile>? Translations { get; set; }
}

/// <summary>Map point inside a post import file. Address translations are keyed by language code.</summary>
public sealed class PostImportLocation
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }

    /// <summary><see cref="Dotnetable.Domain.Enums.PostLocationKind"/> as a number. Omitted when unknown.</summary>
    public byte? Kind { get; set; }

    /// <summary>Address in languages other than the main block.</summary>
    public Dictionary<string, string>? Addresses { get; set; }
}

public static class PostImportWriter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = global::System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static string Write(
        string language,
        PostImportLocale main,
        IEnumerable<KeyValuePair<string, PostImportLocale>> translations,
        PostImportLocation? location = null)
    {
        var file = From(language, main);
        if (location is not null && HasPoint(location)) file.Location = Clean(location);
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

    private static bool HasPoint(PostImportLocation location) =>
        location.Latitude is not null
        || location.Longitude is not null
        || location.Kind is not null
        || !string.IsNullOrWhiteSpace(location.Address)
        || (location.Addresses?.Any(kv => !string.IsNullOrWhiteSpace(kv.Value)) ?? false);

    private static PostImportLocation Clean(PostImportLocation location)
    {
        Dictionary<string, string>? addresses = null;
        if (location.Addresses is not null)
        {
            addresses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (code, address) in location.Addresses)
            {
                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(address)) continue;
                addresses[code.Trim()] = address.Trim();
            }
            if (addresses.Count == 0) addresses = null;
        }

        return new PostImportLocation
        {
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Kind = location.Kind,
            Address = string.IsNullOrWhiteSpace(location.Address) ? null : location.Address.Trim(),
            Addresses = addresses,
        };
    }
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
          "location": {
            "latitude": 35.6892,
            "longitude": 51.389,
            "kind": 8,
            "address": "تهران، خیابان ولیعصر",
            "addresses": {
              "en": "Valiasr Street, Tehran"
            }
          },
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

    public static string PromptFor(string? languageCode) =>
        languageCode is not null && languageCode.StartsWith("fa", StringComparison.OrdinalIgnoreCase)
            ? PromptFa
            : PromptEn;

    public const string PromptFa = """
        فقط یک شیء JSON معتبر برگردان. قبل و بعدش توضیح، مارک‌داون و حصار کد نگذار.

        اگر پایین این پیام مقاله‌ای آمده، همان را به این شکل دربیاور. اگر مقاله‌ای نیست، خودت یک مقالهٔ کامل بنویس و خروجی را فقط به صورت همین JSON بده.

        بلوک اصلی، زبان مقاله است. language کد آن زبان است، مثلاً fa. هر زبان دیگر را داخل translations با کلید همان کد بگذار، مثلاً en. هر بلوک این فیلدها را دارد:
        - title
        - slug فقط با حروف انگلیسی کوچک، عدد و خط تیره
        - summary خلاصهٔ کوتاه
        - meta_title
        - meta_description حدود ۱۵۰ نویسه
        - keywords واژه‌ها با ویرگول
        - content_html متن مقاله با HTML (p و h2 و h3 و ul). تگ‌های html و body را نگذار.

        تصویر شاخص، دسته و برچسب را در فایل نگذار.

        اگر مقاله دربارهٔ یک مکان مشخص است، location را اضافه کن:
        - latitude و longitude عدد اعشاری‌اند. فقط وقتی مختصات را مطمئن هستی بنویس و مختصات را حدس نزن. یا هر دو را بگذار یا هیچ‌کدام را.
        - address آدرس به زبان بلوک اصلی است.
        - addresses یک شیء است: کلید، کد زبان و مقدار، آدرس همان زبان. زبان اصلی را اینجا تکرار نکن.
        - kind اختیاری و عدد است: ۰ نامشخص، ۱ دفتر، ۲ فروشگاه، ۳ شعبه، ۴ شوروم، ۵ انبار، ۶ محل برگزاری، ۷ سایر، ۸ تاریخی و فرهنگی، ۹ تفریحی، ۱۰ اقامتگاه بوم‌گردی، ۱۱ کوله‌گردی، ۱۲ رستوران، ۱۳ زیارتی و مذهبی، ۱۴ طبیعت‌گردی، ۱۵ کمپ و ماجراجویی، ۱۶ بازار سنتی. اگر نوع مکان معلوم نیست kind را حذف کن.

        اگر مقاله مکان ندارد، location را ننویس.

        مقاله را بعد از این دستور بچسبان، یا موضوع را بنویس تا خودت مقاله را بسازی.
        """;

    public const string PromptEn = """
        Return only one valid JSON object. Do not add an explanation, Markdown, or a code fence before or after it.

        If an article follows this message, convert that article into the JSON. If no article follows, write a complete article yourself and return only this JSON.

        The root object is the article's main language. language is that language code, for example en. Put every other language in translations, keyed by its code, for example fa. Each block has:
        - title
        - slug, using only lowercase English letters, digits, and hyphens
        - summary, a short excerpt
        - meta_title
        - meta_description, about 150 characters
        - keywords, separated by commas
        - content_html, the article as HTML (p, h2, h3, ul). Do not wrap it in html or body tags.

        Do not include a featured image, categories, or tags.

        When the article is about a specific place, add location:
        - latitude and longitude are decimal numbers. Include them only when you know the coordinates. Do not guess. Include both or neither.
        - address is the address in the main language.
        - addresses is an object: the key is a language code and the value is the address in that language. Do not repeat the main language here.
        - kind is optional and numeric: 0 Unspecified, 1 Office, 2 Store, 3 Branch, 4 Showroom, 5 Warehouse, 6 Venue, 7 Other, 8 Historical and cultural, 9 Recreation, 10 Ecolodge, 11 Backpacking, 12 Restaurant, 13 Pilgrimage, 14 Nature, 15 Camp and adventure, 16 Bazaar. Omit kind when the place type is unknown.

        When the article has no place, omit location.

        Paste the article after this instruction, or name the subject and ask for the article to be written.
        """;
}
