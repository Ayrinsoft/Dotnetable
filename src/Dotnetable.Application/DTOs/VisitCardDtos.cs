namespace Dotnetable.Application.DTOs;

/// <summary>Text of one extra language on a visiting card. Blank fields fall back to the primary text.</summary>
public sealed class VisitCardTranslation
{
    public string? Lang { get; set; }
    public string? FullName { get; set; }
    public string? BrandName { get; set; }
    public string? JobTitle { get; set; }
    public string? Slogan { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
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

    public bool HasText =>
        FullName is not null || BrandName is not null || JobTitle is not null || Slogan is not null
        || Address is not null || Description is not null
        || Field1Title is not null || Field1Value is not null
        || Field2Title is not null || Field2Value is not null
        || Field3Title is not null || Field3Value is not null
        || Field4Title is not null || Field4Value is not null
        || Field5Title is not null || Field5Value is not null;
}

/// <summary>One map pin on a visiting card.</summary>
public sealed class VisitCardLocation
{
    public string? Title { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
}

/// <summary>What the admin editor loads and saves for one member's visiting card.</summary>
public sealed class AuthorVisitCardEdit
{
    public bool Enabled { get; set; }
    public bool PhoneBookEnabled { get; set; } = true;
    public string? Slug { get; set; }
    public string? FullName { get; set; }
    public string? BrandName { get; set; }
    public string? JobTitle { get; set; }
    public string? Slogan { get; set; }
    public string? Mobile { get; set; }
    public string? Phone { get; set; }
    public string? WhatsApp { get; set; }
    public string? Address { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public int? LogoFileID { get; set; }
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
    public bool IsPortrait { get; set; } = true;
    public byte CardLayout { get; set; }
    public byte ThemeMode { get; set; }
    public string? Description { get; set; }
    public string? Bank1HolderName { get; set; }
    public string? Bank1Details { get; set; }
    public string? Bank1Sheba { get; set; }
    public string? Bank1CardNumber { get; set; }
    public string? Bank2HolderName { get; set; }
    public string? Bank2Details { get; set; }
    public string? Bank2Sheba { get; set; }
    public string? Bank2CardNumber { get; set; }
    public List<VisitCardTranslation> Translations { get; set; } = [];
    public List<AuthorSocialLink> SocialLinks { get; set; } = [];
    public List<VisitCardLocation> Locations { get; set; } = [];
}

/// <summary>Admin editor state: the card plus the bits the form needs to preview files and the public link.</summary>
public sealed class AuthorVisitCardEditorState
{
    public bool MemberFound { get; init; }
    public int WebsiteId { get; init; }
    public string? WebsiteAddress { get; init; }
    public string? LogoUrl { get; init; }
    public string? CoverUrl { get; init; }
    public AuthorVisitCardEdit Card { get; init; } = new();
}

/// <summary>Public visiting card. Translations stay in the payload so the page can switch language without a reload.</summary>
public sealed class VisitCardPageDto
{
    public string Slug { get; init; } = string.Empty;
    public string? WebsiteName { get; init; }
    public string? ResumeSlug { get; init; }
    public bool PhoneBookEnabled { get; init; }
    public bool IsPortrait { get; init; }
    public byte CardLayout { get; init; }
    public byte ThemeMode { get; init; }
    public string CardColor { get; init; } = "#1976d2";
    public string PageColor { get; init; } = "#0d1b2a";
    public string? FullName { get; init; }
    public string? BrandName { get; init; }
    public string? JobTitle { get; init; }
    public string? Slogan { get; init; }
    public string? Mobile { get; init; }
    public string? Phone { get; init; }
    public string? WhatsApp { get; init; }
    public string? Address { get; init; }
    public string? Website { get; init; }
    public string? Email { get; init; }
    public string? LogoUrl { get; init; }
    public string? CoverUrl { get; init; }
    public string? Description { get; init; }
    public string? Field1Title { get; init; }
    public string? Field1Value { get; init; }
    public string? Field2Title { get; init; }
    public string? Field2Value { get; init; }
    public string? Field3Title { get; init; }
    public string? Field3Value { get; init; }
    public string? Field4Title { get; init; }
    public string? Field4Value { get; init; }
    public string? Field5Title { get; init; }
    public string? Field5Value { get; init; }
    public string? Bank1HolderName { get; init; }
    public string? Bank1Details { get; init; }
    public string? Bank1Sheba { get; init; }
    public string? Bank1CardNumber { get; init; }
    public string? Bank2HolderName { get; init; }
    public string? Bank2Details { get; init; }
    public string? Bank2Sheba { get; init; }
    public string? Bank2CardNumber { get; init; }
    public IReadOnlyList<AuthorSocialLink> SocialLinks { get; init; } = Array.Empty<AuthorSocialLink>();
    public IReadOnlyList<VisitCardLocation> Locations { get; init; } = Array.Empty<VisitCardLocation>();
    public IReadOnlyList<VisitCardTranslation> Translations { get; init; } = Array.Empty<VisitCardTranslation>();
}

public sealed class VisitCardContactRequest
{
    public string? Name { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Reason { get; set; }
    public string? VisitorKey { get; set; }
    public bool SavedContact { get; set; }

    /// <summary>Honeypot. Left empty by a person; a bot that fills every field is dropped.</summary>
    public string? Company { get; set; }
}

public sealed class VisitCardContactItem
{
    public int ContactID { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? Mobile { get; init; }
    public string? Email { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Note { get; init; }
    public bool SavedContact { get; init; }
    public DateTime LogTime { get; init; }
}

public sealed class VisitCardPhoneBookPage
{
    public bool HasCard { get; init; }
    public bool PhoneBookEnabled { get; init; }
    public string? Slug { get; init; }
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public IReadOnlyList<VisitCardContactItem> Items { get; init; } = Array.Empty<VisitCardContactItem>();
}
