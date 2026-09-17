namespace Dotnetable.Domain.Enums;

/// <summary>
/// Section an <see cref="Entities.AuthorResumeItem"/> belongs to on an author's online résumé.
/// Stored as a TINYINT in <see cref="Entities.AuthorResumeItem.ItemType"/>; the values are persisted,
/// so never renumber them.
/// </summary>
public enum ResumeItemType : byte
{
    /// <summary>A job / position held.</summary>
    Experience = 0,

    /// <summary>A degree, course of study or school.</summary>
    Education = 1,

    /// <summary>A piece of work the author wants to show off (a product, open-source project, client job).</summary>
    Project = 2,

    /// <summary>A certificate or licence.</summary>
    Certification = 3,

    /// <summary>An award, honour or prize.</summary>
    Award = 4,

    /// <summary>A book, paper, talk or article published elsewhere.</summary>
    Publication = 5,

    /// <summary>Volunteer or community work.</summary>
    Volunteering = 6,

    /// <summary>Anything else worth a line on the timeline.</summary>
    Other = 9,
}
