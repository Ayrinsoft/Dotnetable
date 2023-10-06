namespace Dotnetable.Shared.DTO.Public;

public class PaginationModel
{
    public int PageIndex { get; set; } = 1;
    public int MaxLength { get; set; } = 0;
    public int PageSize { get; set; } = 10;
    public bool ShowNumbers { get; set; } = true;
    public bool ShowFirstLast { get; set; } = false;
    public int VisiblePages { get; set; } = 5;
}
