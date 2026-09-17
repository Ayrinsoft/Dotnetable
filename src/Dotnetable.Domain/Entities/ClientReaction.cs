using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// A signed-in customer's like/favorite or bookmark on a product, post or page. One row per
/// (client, target, reaction) — the unique index is what guarantees a like is counted once per client.
/// The target is polymorphic (<see cref="TargetType"/> + <see cref="TargetID"/>), so there is no FK to it:
/// the product/post/page delete paths remove their reactions explicitly.
/// </summary>
public partial class ClientReaction
{
    public int ClientReactionID { get; set; }

    public int WebsiteID { get; set; }

    public int WebsiteClientID { get; set; }

    /// <summary><see cref="Enums.ReactionTargetType"/>.</summary>
    public byte TargetType { get; set; }

    public int TargetID { get; set; }

    /// <summary><see cref="Enums.ReactionType"/>.</summary>
    public byte ReactionType { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual WebsiteClient WebsiteClient { get; set; } = null!;
}
