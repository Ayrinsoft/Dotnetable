using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// Optional digital visiting card of an admin member who writes content. Public at <c>/c/{Slug}</c>
/// when <see cref="Enabled"/> is true. One row per member.
/// </summary>
public partial class AuthorVisitCard
{
    public int AuthorVisitCardID { get; set; }

    public int MemberID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>When false the short link returns 404. The slug stays reserved.</summary>
    public bool Enabled { get; set; }

    /// <summary>Short public address, for example <c>aliu</c> → <c>/c/aliu</c>. Unique per website.</summary>
    public string? Slug { get; set; }

    public string? FullName { get; set; }

    public string? BrandName { get; set; }

    public string? JobTitle { get; set; }

    public string? Slogan { get; set; }

    public string? Mobile { get; set; }

    public string? Phone { get; set; }

    /// <summary>Label the owner writes for the first extra number, for example company phone.</summary>
    public string? Phone2Title { get; set; }

    public string? Phone2 { get; set; }

    /// <summary>Label the owner writes for the second extra number.</summary>
    public string? Phone3Title { get; set; }

    public string? Phone3 { get; set; }

    public string? WhatsApp { get; set; }

    public string? Address { get; set; }

    public string? WebsiteUrl { get; set; }

    public string? Email { get; set; }

    public int? LogoFileID { get; set; }

    /// <summary>Wide photo behind the banner layout.</summary>
    public int? CoverFileID { get; set; }

    public string? CardColor { get; set; }

    public string? PageColor { get; set; }

    public string? Field1Title { get; set; }
    public string? Field1Value { get; set; }
    public string? Field2Title { get; set; }
    public string? Field2Value { get; set; }
    public string? Field3Title { get; set; }
    public string? Field3Value { get; set; }
    public string? Field4Title { get; set; }
    public string? Field4Value { get; set; }
    public string? Field5Title { get; set; }
    public string? Field5Value { get; set; }

    /// <summary>true = portrait (default), false = landscape. Applies to the flip layout.</summary>
    public bool IsPortrait { get; set; }

    /// <summary>0 flip card, 1 photo banner, 2 portrait profile.</summary>
    public byte CardLayout { get; set; }

    /// <summary>0 dark, 1 light. The visitor can switch it on the page.</summary>
    public byte ThemeMode { get; set; }

    /// <summary>Plain-text bio shown on the back of the flip card (and under the other layouts).</summary>
    public string? Description { get; set; }

    /// <summary>JSON array of <c>VisitCardTranslation</c>. Empty languages are dropped on save.</summary>
    public string? TranslationsJson { get; set; }

    /// <summary>JSON array of <c>{ "network", "url" }</c>, same shape as an author profile.</summary>
    public string? SocialLinksJson { get; set; }

    /// <summary>JSON array of <c>{ "title", "lat", "lng" }</c> map pins.</summary>
    public string? LocationsJson { get; set; }

    /// <summary>When true, visitors can leave their number, email and a reason.</summary>
    public bool PhoneBookEnabled { get; set; }

    public string? Bank1HolderName { get; set; }
    public string? Bank1Details { get; set; }
    public string? Bank1Sheba { get; set; }
    public string? Bank1CardNumber { get; set; }
    public string? Bank2HolderName { get; set; }
    public string? Bank2Details { get; set; }
    public string? Bank2Sheba { get; set; }
    public string? Bank2CardNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Member Member { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;

    public virtual FileRecord? LogoFile { get; set; }

    public virtual FileRecord? CoverFile { get; set; }

    public virtual ICollection<AuthorVisitCardContact> Contacts { get; set; } = new List<AuthorVisitCardContact>();
}
