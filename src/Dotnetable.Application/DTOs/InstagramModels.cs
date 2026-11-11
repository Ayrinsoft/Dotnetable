using Dotnetable.Application.Interfaces;

namespace Dotnetable.Application.DTOs;

/// <summary>What the admin may see about the connected Instagram account. The token itself is never returned.</summary>
public sealed class InstagramAccountStatus
{
    public int AccountId { get; init; }
    public string Title { get; init; } = "";
    public string? LanguageCode { get; init; }
    public bool Connected { get; init; }
    public string? IgUserId { get; init; }
    public string? Username { get; init; }
    public string? PageId { get; init; }
    public bool HasToken { get; init; }
}

/// <summary>Fields the admin saves. A blank token keeps the token already stored.</summary>
public sealed class InstagramAccountInput
{
    /// <summary>Zero adds a channel. A positive id updates that channel.</summary>
    public int AccountId { get; set; }
    public string Title { get; set; } = "";
    public string? LanguageCode { get; set; }
    public string IgUserId { get; set; } = "";
    public string? AccessToken { get; set; }
    public string? PageId { get; set; }
}

/// <summary>One photo in the public post payload. The first item is the cover.</summary>
public sealed class PostImageDto
{
    public string Url { get; init; } = "";
    public string? FullUrl { get; init; }
    public int SortOrder { get; init; }
}

public enum InstagramMediaKind
{
    Image = 0,
    Video = 1,
}

/// <summary>One slide the admin wants on the Instagram post. A public URL, a library file, or uploaded bytes.</summary>
public sealed class InstagramMediaItem
{
    public InstagramMediaKind Kind { get; set; }
    public string? Url { get; set; }
    public int? FileRecordId { get; set; }
    public byte[]? Content { get; set; }
    public string? FileName { get; set; }
    public string? MimeType { get; set; }
    public ImageCropRect? Crop { get; set; }

    /// <summary>Included when <see cref="InstagramPublishRequest.SendInstagram"/> is on.</summary>
    public bool ToInstagram { get; set; } = true;

    /// <summary>Included when <see cref="InstagramPublishRequest.SendTelegram"/> is on.</summary>
    public bool ToTelegram { get; set; } = true;

    /// <summary>Included when <see cref="InstagramPublishRequest.SendX"/> is on.</summary>
    public bool ToX { get; set; } = true;
}

/// <summary>Popup payload. <see cref="UsePrimaryExcerpt"/> reads the post's main-language summary, not a translation.</summary>
public sealed class InstagramPublishRequest
{
    public string? Caption { get; set; }
    public bool UsePrimaryExcerpt { get; set; }
    public List<InstagramMediaItem> Items { get; set; } = new();

    /// <summary>Index of the slide that should be the feed cover (moved first). Ignored when there is a single video.</summary>
    public int CoverIndex { get; set; }

    /// <summary>Thumbnail for a single video. Not added as its own slide. Instagram only.</summary>
    public InstagramMediaItem? VideoCover { get; set; }

    public bool SendInstagram { get; set; }

    public bool SendTelegram { get; set; }

    public bool SendX { get; set; }

    /// <summary>Instagram channel to use. Null uses the only saved channel.</summary>
    public int? InstagramAccountId { get; set; }

    /// <summary>Telegram channel to use. Null uses the only saved channel.</summary>
    public int? TelegramAccountId { get; set; }

    /// <summary>X channel to use. Null uses the only saved channel.</summary>
    public int? XAccountId { get; set; }

    /// <summary>Translation language for the summary. Null or a missing translation uses the primary text.</summary>
    public string? ContentLanguageCode { get; set; }

    /// <summary>Keywords the admin left ticked. Stored phrases, not hashtags. Empty sends none.</summary>
    public List<string> Hashtags { get; set; } = new();

    /// <summary>When true, the public address is replaced with <c>/l/{code}</c> before it is appended.</summary>
    public bool ShortenLink { get; set; }

    /// <summary>When true, <see cref="Caption"/> is the whole message. The link and hashtags are not added again.</summary>
    public bool SkipAppend { get; set; }
}
