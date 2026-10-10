namespace Dotnetable.Domain.Entities;

/// <summary>
/// A visitor report about something they saw on the public site: a bug, a broken page,
/// wrong content, or content that should not be public.
/// </summary>
public partial class SiteReport
{
    public int SiteReportID { get; set; }

    public int WebsiteID { get; set; }

    public int? WebsiteClientID { get; set; }

    public string ReporterName { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;

    /// <summary><see cref="Enums.SiteReportKind"/>.</summary>
    public byte Kind { get; set; }

    public string Subject { get; set; } = null!;

    public string MessageBody { get; set; } = null!;

    public string? PageUrl { get; set; }

    public string SenderIPAddress { get; set; } = null!;

    /// <summary><see cref="Enums.SiteReportStatus"/>.</summary>
    public byte Status { get; set; }

    public string? AdminNote { get; set; }

    public DateTime LogTime { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual WebsiteClient? WebsiteClient { get; set; }
}
