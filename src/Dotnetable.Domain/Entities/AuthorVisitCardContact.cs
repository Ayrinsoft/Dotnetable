using System;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// One row in a visiting card's phone book: a visitor who left a number and/or email and said why
/// they should be called back. The card owner renames them and adds a short note.
/// </summary>
public partial class AuthorVisitCardContact
{
    public int ContactID { get; set; }

    public int AuthorVisitCardID { get; set; }

    public string Name { get; set; } = null!;

    public string? Mobile { get; set; }

    public string? Email { get; set; }

    public string Reason { get; set; } = null!;

    public DateTime LogTime { get; set; }

    /// <summary>Name the card owner prefers to see. The visitor's <see cref="Name"/> stays as submitted.</summary>
    public string? DisplayName { get; set; }

    public string? Note { get; set; }

    /// <summary>32 hex chars identifying the browser, so a later submit updates the same person.</summary>
    public string? VisitorKey { get; set; }

    /// <summary>The visitor downloaded the card owner's contact file.</summary>
    public bool SavedContact { get; set; }

    public virtual AuthorVisitCard AuthorVisitCard { get; set; } = null!;
}
