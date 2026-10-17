using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.VisitCards;

/// <summary>
/// Normalises a digital visiting card before it is stored, and builds the contact file visitors download.
/// Mirrors the checks used by the PersianQR visiting card (slug, banks, phones) and adds the visitor email.
/// </summary>
public static class VisitCardRules
{
    public const int SlugMin = 2;
    public const int SlugMax = 32;
    public const int ContactCapacity = 5000;
    public const int ReasonMax = 300;
    public const int ReasonStoreMax = 2000;

    private static readonly Regex SlugRegex = new("^[a-z][a-z0-9]*(-[a-z0-9]+)*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex HexColor = new("^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$", RegexOptions.Compiled);
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>Languages a card can add besides its primary (Persian) text. Code → (native name, right-to-left).</summary>
    public static readonly IReadOnlyDictionary<string, (string Name, bool Rtl)> Languages =
        new Dictionary<string, (string, bool)>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = ("English", false),
            ["ar"] = ("العربية", true),
            ["tr"] = ("Türkçe", false),
            ["de"] = ("Deutsch", false),
            ["fr"] = ("Français", false),
            ["ru"] = ("Русский", false),
        };

    public static readonly IReadOnlyList<string> SocialNetworks =
    [
        "website", "linkedin", "github", "x", "instagram", "telegram", "youtube", "facebook",
        "whatsapp", "aparat", "bale", "rubika", "eitaa", "mastodon", "stackoverflow", "dribbble", "behance", "medium",
    ];

    private static readonly HashSet<string> ReservedSlugs = new(StringComparer.OrdinalIgnoreCase)
    {
        "api", "admin", "www", "app", "mail", "blog", "faq", "author", "c", "page", "shop",
        "cart", "checkout", "account", "contact", "files", "media", "login", "signin", "null",
    };

    public static bool TryNormalizeSlug(string? raw, out string? slug, out string? error)
    {
        slug = null;
        error = null;
        if (string.IsNullOrWhiteSpace(raw))
            return true;

        slug = raw.Trim().ToLowerInvariant();
        if (slug.Length < SlugMin || slug.Length > SlugMax || !SlugRegex.IsMatch(slug))
        {
            slug = null;
            error = "Short link must be 2–32 characters, start with a letter, and use only letters, digits and hyphens. Example: aliu";
            return false;
        }

        if (ReservedSlugs.Contains(slug))
        {
            slug = null;
            error = "This short link is reserved. Choose another.";
            return false;
        }

        return true;
    }

    /// <summary>A starting slug from a username or name. May still collide; the service adds a suffix.</summary>
    public static string? SuggestSlug(string? username, string? givenName, string? surname)
    {
        foreach (var candidate in new[] { username, $"{givenName}{surname}", givenName })
        {
            if (string.IsNullOrWhiteSpace(candidate)) continue;
            var folded = new string(candidate.Trim().ToLowerInvariant().Select(c => char.IsAsciiLetterOrDigit(c) ? c : '-').ToArray()).Trim('-');
            while (folded.Contains("--", StringComparison.Ordinal))
                folded = folded.Replace("--", "-", StringComparison.Ordinal);
            if (TryNormalizeSlug(folded, out var slug, out _) && slug is not null)
                return slug;
        }
        return null;
    }

    public static string? TrimTo(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var text = value.Trim();
        return text.Length <= max ? text : text[..max];
    }

    public static string? NormalizeColor(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null
        : HexColor.IsMatch(value.Trim()) ? value.Trim() : null;

    public static string NormalizeDigits(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var builder = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            if (c is >= '۰' and <= '۹') builder.Append((char)('0' + (c - '۰')));
            else if (c is >= '٠' and <= '٩') builder.Append((char)('0' + (c - '٠')));
            else if (c is '‌' or '‎' or '‏' or '؜' or ' ') continue;
            else builder.Append(c);
        }
        return builder.ToString();
    }

    public static string NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
        return string.Concat(NormalizeDigits(phone).Trim().Where(c => char.IsAsciiDigit(c) || c == '+'));
    }

    public static bool IsPhone(string phone) => Regex.IsMatch(phone, @"^\+?[0-9]{8,18}$");

    /// <summary>Keeps a titled extra number only when the number itself is present and valid.</summary>
    public static string? PrepareExtraPhone(string which, string? title, string? number, out string? titleOut, out string? numberOut)
    {
        titleOut = null;
        numberOut = NormalizePhone(number);
        if (numberOut.Length == 0)
        {
            numberOut = null;
            return null;
        }

        if (!IsPhone(numberOut))
            return $"The {which} is not a valid phone number.";

        if (numberOut.Length > 20) numberOut = numberOut[..20];
        titleOut = TrimTo(title, 50);
        return null;
    }

    public static string ExtraPhoneTitle(string? title) =>
        string.IsNullOrWhiteSpace(title) ? "شماره" : title.Trim();

    /// <summary>Same Iranian mobile written as 0912…, 912… or +98912… shares one key.</summary>
    public static string MobileKey(string? mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile)) return string.Empty;
        var digits = new string(NormalizeDigits(mobile).Where(char.IsAsciiDigit).ToArray());
        if (digits.StartsWith("00", StringComparison.Ordinal)) digits = digits[2..];
        if (digits.StartsWith("98", StringComparison.Ordinal) && digits.Length >= 12) return digits;
        if (digits.StartsWith('0') && digits.Length >= 10) return "98" + digits[1..];
        if (digits.Length == 10 && digits.StartsWith('9')) return "98" + digits;
        return digits;
    }

    public static string AppendReason(string? current, string? addition, int maxLength = ReasonStoreMax)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(current))
        {
            foreach (var line in current.Split('\n'))
            {
                var item = line.Trim();
                if (item.Length > 0 && (parts.Count == 0 || parts[^1] != item))
                    parts.Add(item);
            }
        }

        if (!string.IsNullOrWhiteSpace(addition))
        {
            foreach (var line in addition.Split('\n'))
            {
                var item = line.Trim();
                if (item.Length > 0 && (parts.Count == 0 || parts[^1] != item))
                    parts.Add(item);
            }
        }

        var text = string.Join('\n', parts);
        return text.Length <= maxLength ? text : text[..maxLength];
    }

    /// <summary>
    /// Validates a visitor submission. Returns an error code (<c>name</c>, <c>contact</c>, <c>mobile</c>,
    /// <c>email</c>, <c>reason</c>) or null when it is acceptable. Outputs the stored values.
    /// </summary>
    public static string? ValidateContact(string? name, string? mobile, string? email, string? reason,
        out string nameOut, out string? mobileOut, out string? emailOut, out string reasonOut)
    {
        nameOut = (name ?? string.Empty).Trim();
        mobileOut = NormalizePhone(mobile);
        emailOut = (email ?? string.Empty).Trim();
        reasonOut = (reason ?? string.Empty).Trim();
        if (mobileOut.Length == 0) mobileOut = null;
        if (emailOut.Length == 0) emailOut = null;

        if (nameOut.Length < 2 || nameOut.Length > 70) return "name";
        if (reasonOut.Length < 2 || reasonOut.Length > ReasonMax) return "reason";
        if (mobileOut is null && emailOut is null) return "contact";
        if (mobileOut is not null && !IsPhone(mobileOut)) return "mobile";
        if (emailOut is not null && (emailOut.Length > 120 || !EmailRegex.IsMatch(emailOut))) return "email";
        return null;
    }

    public static string? NormalizeVisitorKey(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var key = raw.Trim().ToLowerInvariant();
        return key.Length == 32 && key.All(Uri.IsHexDigit) ? key : null;
    }

    /// <summary>Writes the two bank accounts back onto <paramref name="card"/>. Returns an error or null.</summary>
    public static string? PrepareBanks(AuthorVisitCardEdit card)
    {
        var error = PrepareBank("first", card.Bank1HolderName, card.Bank1Details, card.Bank1Sheba, card.Bank1CardNumber,
            out var n1, out var d1, out var s1, out var c1);
        if (error is not null) return error;
        error = PrepareBank("second", card.Bank2HolderName, card.Bank2Details, card.Bank2Sheba, card.Bank2CardNumber,
            out var n2, out var d2, out var s2, out var c2);
        if (error is not null) return error;
        card.Bank1HolderName = n1; card.Bank1Details = d1; card.Bank1Sheba = s1; card.Bank1CardNumber = c1;
        card.Bank2HolderName = n2; card.Bank2Details = d2; card.Bank2Sheba = s2; card.Bank2CardNumber = c2;
        return null;
    }

    public static bool IsValidIranSheba(string sheba)
    {
        if (sheba.Length != 26 || !sheba.StartsWith("IR", StringComparison.Ordinal)) return false;
        for (var i = 2; i < sheba.Length; i++)
            if (!char.IsDigit(sheba[i])) return false;

        var rearranged = sheba[4..] + sheba[..4];
        var mod = 0;
        foreach (var ch in rearranged)
        {
            var value = char.IsDigit(ch) ? ch - '0' : ch - 'A' + 10;
            if (value >= 10)
                mod = (mod * 10 + (value / 10)) % 97;
            mod = (mod * 10 + (value % 10)) % 97;
        }
        return mod == 1;
    }

    public static string? SerializeTranslations(IEnumerable<VisitCardTranslation>? items)
    {
        var clean = new List<VisitCardTranslation>();
        foreach (var t in items ?? [])
        {
            var lang = t.Lang?.Trim().ToLowerInvariant();
            if (lang is null || !Languages.ContainsKey(lang) || clean.Any(c => c.Lang == lang)) continue;
            var row = new VisitCardTranslation
            {
                Lang = lang,
                FullName = TrimTo(t.FullName, 70),
                BrandName = TrimTo(t.BrandName, 70),
                JobTitle = TrimTo(t.JobTitle, 60),
                Slogan = TrimTo(t.Slogan, 160),
                Address = TrimTo(t.Address, 250),
                Description = TrimTo(t.Description, 4000),
                Field1Title = TrimTo(t.Field1Title, 50), Field1Value = TrimTo(t.Field1Value, 250),
                Field2Title = TrimTo(t.Field2Title, 50), Field2Value = TrimTo(t.Field2Value, 250),
                Field3Title = TrimTo(t.Field3Title, 50), Field3Value = TrimTo(t.Field3Value, 250),
                Field4Title = TrimTo(t.Field4Title, 50), Field4Value = TrimTo(t.Field4Value, 250),
                Field5Title = TrimTo(t.Field5Title, 50), Field5Value = TrimTo(t.Field5Value, 250),
            };
            if (row.HasText) clean.Add(row);
        }
        return clean.Count == 0 ? null : JsonSerializer.Serialize(clean, Json);
    }

    public static List<VisitCardTranslation> ParseTranslations(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<VisitCardTranslation>>(json, Json) ?? []; }
        catch (JsonException) { return []; }
    }

    public static string? SerializeSocials(IEnumerable<AuthorSocialLink>? links)
    {
        var clean = (links ?? [])
            .Where(l => AuthorSocialLinks.IsSafeUrl(l.Url))
            .Select(l => new AuthorSocialLink
            {
                Network = SocialNetworks.Contains(l.Network?.Trim().ToLowerInvariant() ?? "", StringComparer.Ordinal)
                    ? l.Network!.Trim().ToLowerInvariant() : "website",
                Url = l.Url.Trim(),
            })
            .Take(12)
            .ToList();
        return clean.Count == 0 ? null : JsonSerializer.Serialize(clean, Json);
    }

    public static List<AuthorSocialLink> ParseSocials(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<AuthorSocialLink>>(json, Json) ?? []; }
        catch (JsonException) { return []; }
    }

    public static string? SerializeLocations(IEnumerable<VisitCardLocation>? locations)
    {
        var clean = new List<VisitCardLocation>();
        foreach (var loc in locations ?? [])
        {
            if (loc.Lat is < -90 or > 90 || loc.Lng is < -180 or > 180) continue;
            if (loc.Lat == 0 && loc.Lng == 0) continue;
            clean.Add(new VisitCardLocation { Title = TrimTo(loc.Title, 80), Lat = loc.Lat, Lng = loc.Lng });
            if (clean.Count == 5) break;
        }
        return clean.Count == 0 ? null : JsonSerializer.Serialize(clean, Json);
    }

    public static List<VisitCardLocation> ParseLocations(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<VisitCardLocation>>(json, Json) ?? []; }
        catch (JsonException) { return []; }
    }

    public static string? NormalizeWebsite(string? raw)
    {
        var text = TrimTo(raw, 200);
        if (text is null) return null;
        if (!text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !text.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            text = "https://" + text;
        return AuthorSocialLinks.IsSafeUrl(text) ? text : null;
    }

    public static string BuildVcf(string? fullName, string? brand, string? jobTitle, string? mobile, string? phone,
        string? email, string? website, string? address, string? note,
        string? phone2 = null, string? phone2Title = null, string? phone3 = null, string? phone3Title = null)
    {
        var lines = new List<string> { "BEGIN:VCARD", "VERSION:3.0" };
        var name = string.IsNullOrWhiteSpace(fullName) ? brand : fullName;
        if (string.IsNullOrWhiteSpace(name)) name = "Contact";
        lines.Add("FN:" + VcfEscape(name));
        lines.Add("N:" + VcfEscape(name) + ";;;;");
        if (!string.IsNullOrWhiteSpace(brand)) lines.Add("ORG:" + VcfEscape(brand));
        if (!string.IsNullOrWhiteSpace(jobTitle)) lines.Add("TITLE:" + VcfEscape(jobTitle));
        if (!string.IsNullOrWhiteSpace(mobile)) lines.Add("TEL;TYPE=CELL:" + VcfEscape(mobile));
        if (!string.IsNullOrWhiteSpace(phone)) lines.Add("TEL;TYPE=WORK:" + VcfEscape(phone));
        AddLabeledTel(lines, phone2, phone2Title);
        AddLabeledTel(lines, phone3, phone3Title);
        if (!string.IsNullOrWhiteSpace(email)) lines.Add("EMAIL:" + VcfEscape(email));
        if (!string.IsNullOrWhiteSpace(website)) lines.Add("URL:" + VcfEscape(website));
        if (!string.IsNullOrWhiteSpace(address)) lines.Add("ADR;TYPE=WORK:;;" + VcfEscape(address) + ";;;;");
        if (!string.IsNullOrWhiteSpace(note)) lines.Add("NOTE:" + VcfEscape(note));
        lines.Add("END:VCARD");
        return string.Join("\r\n", lines);
    }

    private static void AddLabeledTel(List<string> lines, string? number, string? title)
    {
        if (string.IsNullOrWhiteSpace(number)) return;
        var id = "item" + lines.Count;
        lines.Add(id + ".TEL;TYPE=VOICE:" + VcfEscape(number));
        lines.Add(id + ".X-ABLabel:" + VcfEscape(ExtraPhoneTitle(title)));
    }

    public static string VcfEscape(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\r\n", "\\n", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace(",", "\\,", StringComparison.Ordinal)
            .Replace(";", "\\;", StringComparison.Ordinal);

    /// <summary>White text on a dark card colour, dark text on a light one.</summary>
    public static string ContrastInk(string? hex)
    {
        hex = (hex ?? string.Empty).Trim().TrimStart('#');
        if (hex.Length == 3) hex = string.Concat(hex.Select(c => new string(c, 2)));
        if (hex.Length != 6 || !int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
            return "#ffffff";
        int r = Convert.ToInt32(hex[..2], 16);
        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
        int b = Convert.ToInt32(hex.Substring(4, 2), 16);
        var y = (0.299 * r + 0.587 * g + 0.114 * b) / 255.0;
        return y > 0.62 ? "#162033" : "#ffffff";
    }

    private static string? PrepareBank(string which, string? holder, string? details, string? sheba, string? card,
        out string? holderOut, out string? detailsOut, out string? shebaOut, out string? cardOut)
    {
        holderOut = detailsOut = shebaOut = cardOut = null;
        holder = TrimTo(holder, 70);
        details = TrimTo(details, 120);
        sheba = CompactSheba(sheba);
        card = string.Concat(NormalizeDigits(card).Where(char.IsAsciiDigit));

        if (sheba.Length == 0 && card.Length == 0 && holder is null && details is null)
            return null;
        if (sheba.Length == 0 && card.Length == 0)
            return $"Enter an IBAN or a card number for the {which} account.";
        if (holder is null)
            return $"Enter the account holder for the {which} account.";
        if (sheba.Length > 0 && !IsValidIranSheba(sheba))
            return $"The {which} IBAN is not valid. An Iranian IBAN starts with IR and has 24 digits after it.";
        if (card.Length > 0 && card.Length != 16)
            return $"The {which} card number must be 16 digits.";

        holderOut = holder;
        detailsOut = details;
        shebaOut = sheba.Length == 0 ? null : sheba;
        cardOut = card.Length == 0 ? null : card;
        return null;
    }

    private static string CompactSheba(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var compact = new StringBuilder();
        foreach (var c in NormalizeDigits(raw).ToUpperInvariant())
            if (char.IsAsciiLetterOrDigit(c)) compact.Append(c);
        var sheba = compact.ToString();
        if (sheba.Length == 24 && sheba.All(char.IsDigit))
            sheba = "IR" + sheba;
        return sheba;
    }
}
