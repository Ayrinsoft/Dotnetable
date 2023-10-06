namespace Dotnetable.Admin.Models.Nestable;

public class NestableStandardRequest
{
    public string ItemID { get; set; }
    public string ParentID { get; set; }
    public int Priority { get; set; }
    public string Title { get; set; }
    public bool Active { get; set; }
}
