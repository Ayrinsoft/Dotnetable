namespace Dotnetable.Domain.Entities;

public class QuizQuestionImage
{
    public int QuizQuestionImageID { get; set; }

    public int QuizQuestionID { get; set; }

    public int FileRecordID { get; set; }

    public int SortOrder { get; set; }

    public virtual QuizQuestion QuizQuestion { get; set; } = null!;

    public virtual FileRecord FileRecord { get; set; } = null!;
}
