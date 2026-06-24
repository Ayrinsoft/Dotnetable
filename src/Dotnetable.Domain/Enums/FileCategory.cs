namespace Dotnetable.Domain.Enums;

/// <summary>
/// Coarse file classification derived from the MIME type at upload time.
/// Stored in <c>FileRecord.FileCategory</c>.
/// </summary>
public enum FileCategory : byte
{
    Image = 1,
    Document = 2,
    Video = 3,
    Audio = 4,
    Other = 9,
}
