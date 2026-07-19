using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class AdminNotification
{
    public int AdminNotificationID { get; set; }

    public int MemberID { get; set; }

    public int WebsiteID { get; set; }

    public byte NotificationType { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string? ActionUrl { get; set; }

    public int? RelatedEntityID { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Member Member { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;
}
