namespace Dotnetable.Domain.Enums;

/// <summary>
/// What a <see cref="Entities.ClientReaction"/> points at. Stored as a TINYINT in
/// <see cref="Entities.ClientReaction.TargetType"/>; never renumber.
/// </summary>
public enum ReactionTargetType : byte
{
    Product = 1,
    Post = 2,
    Page = 3,
}
