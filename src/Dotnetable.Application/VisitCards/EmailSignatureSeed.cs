using System.Text.Json;
using System.Text.RegularExpressions;
using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.VisitCards;

/// <summary>
/// Builds the JSON the email-signature page reads. The signature is not stored:
/// the browser turns this snapshot of the visiting card into HTML.
/// </summary>
public static class EmailSignatureSeed
{
    private static readonly string[] KnownSocials = ["instagram", "telegram", "whatsapp", "linkedin", "x", "youtube", "aparat"];
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static string ToJson(AuthorVisitCardEdit card, string? logoUrl, string? websiteAddress)
    {
        var cardUrl = PublicCardUrl(websiteAddress, card.Slug);
        var socials = new Dictionary<string, string>(StringComparer.Ordinal);
        var extras = new List<object>();

        var whatsApp = WhatsAppLink(card.WhatsApp);
        if (whatsApp.Length > 0) socials["whatsapp"] = whatsApp;

        foreach (var link in card.SocialLinks)
        {
            var href = Absolute(websiteAddress, link.Url);
            if (href.Length == 0) continue;
            var key = SocialKey(link.Network, href);
            if (key.Length > 0)
            {
                socials.TryAdd(key, href);
                continue;
            }

            if (extras.Count < 4 && !SameLink(href, card.Website) && !SameLink(href, cardUrl))
                extras.Add(new { label = ExtraLabel(link.Network), url = href });
        }

        foreach (var field in CustomFields(card))
        {
            if (extras.Count >= 4) break;
            var href = Absolute(websiteAddress, field.Value);
            if (!href.StartsWith("http", StringComparison.OrdinalIgnoreCase)) continue;
            if (SameLink(href, card.Website) || SameLink(href, cardUrl)) continue;
            extras.Add(new { label = string.IsNullOrWhiteSpace(field.Title) ? "لینک" : field.Title.Trim(), url = href });
        }

        var color = VisitCardRules.NormalizeColor(card.CardColor) ?? "#5F41FB";
        var seed = new
        {
            fullName = (card.FullName ?? "").Trim(),
            jobTitle = (card.JobTitle ?? "").Trim(),
            brand = (card.BrandName ?? "").Trim(),
            slogan = (card.Slogan ?? "").Trim(),
            mobile = (card.Mobile ?? "").Trim(),
            phone = (card.Phone ?? "").Trim(),
            email = (card.Email ?? "").Trim(),
            website = (card.Website ?? "").Trim(),
            address = (card.Address ?? "").Trim(),
            photo = Absolute(websiteAddress, logoUrl),
            color,
            cardUrl,
            ctaLabel = "مشاهده کارت ویزیت",
            ctaUrl = cardUrl,
            showCta = cardUrl.Length > 0,
            socials,
            extras,
        };

        return JsonSerializer.Serialize(seed, Json).Replace("<", "\\u003c", StringComparison.Ordinal);
    }

    public static string PublicCardUrl(string? websiteAddress, string? slug)
    {
        if (string.IsNullOrWhiteSpace(websiteAddress) || string.IsNullOrWhiteSpace(slug)) return "";
        var host = websiteAddress.Trim().TrimEnd('/');
        if (!host.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            && !host.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            host = "https://" + host;
        return host + "/c/" + Uri.EscapeDataString(slug.Trim());
    }

    private static IEnumerable<(string? Title, string? Value)> CustomFields(AuthorVisitCardEdit card)
    {
        yield return (card.Field1Title, card.Field1Value);
        yield return (card.Field2Title, card.Field2Value);
        yield return (card.Field3Title, card.Field3Value);
        yield return (card.Field4Title, card.Field4Value);
        yield return (card.Field5Title, card.Field5Value);
    }

    private static string Absolute(string? websiteAddress, string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "";
        url = url.Trim();
        if (url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            return "";
        if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            return url;
        if (!url.StartsWith('/')) return "";
        var host = PublicCardUrl(websiteAddress, "x");
        if (host.Length == 0) return url;
        var origin = host[..host.LastIndexOf("/c/", StringComparison.Ordinal)];
        return origin + url;
    }

    private static bool SameLink(string? a, string? b)
    {
        if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return false;
        return string.Equals(a.Trim().TrimEnd('/'), b.Trim().TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
    }

    private static string WhatsAppLink(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "";
        var text = raw.Trim();
        if (text.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || text.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return text;
        var digits = Regex.Replace(text, "[^0-9]", "");
        if (digits.StartsWith('0')) digits = "98" + digits[1..];
        return digits.Length < 8 ? "" : "https://wa.me/" + digits;
    }

    private static string SocialKey(string? network, string url)
    {
        var key = (network ?? "").Trim().ToLowerInvariant();
        if (KnownSocials.Contains(key)) return key;
        var blob = (key + " " + url).ToLowerInvariant();
        if (blob.Contains("instagram")) return "instagram";
        if (blob.Contains("t.me") || blob.Contains("telegram")) return "telegram";
        if (blob.Contains("wa.me") || blob.Contains("whatsapp")) return "whatsapp";
        if (blob.Contains("linkedin")) return "linkedin";
        if (blob.Contains("twitter") || blob.Contains("x.com")) return "x";
        if (blob.Contains("youtube") || blob.Contains("youtu.be")) return "youtube";
        if (blob.Contains("aparat")) return "aparat";
        return "";
    }

    private static string ExtraLabel(string? network) => (network ?? "").Trim().ToLowerInvariant() switch
    {
        "github" => "گیت‌هاب",
        "facebook" => "فیس‌بوک",
        "bale" => "بله",
        "rubika" => "روبیکا",
        "eitaa" => "ایتا",
        "website" or "" => "لینک",
        _ => network!.Trim(),
    };
}
