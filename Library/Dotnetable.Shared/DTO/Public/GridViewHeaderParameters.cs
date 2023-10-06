namespace Dotnetable.Shared.DTO.Public;

public class GridViewHeaderParameters
{
    public List<HeaderItemDetail> HeaderItems { get; set; }
    public string OrderbyParams { get; set; } = "";
    public bool HasRowNumber { get; set; } = false;
    public PaginationModel Pagination { get; set; }

    public class HeaderItemDetail
    {
        public string ColumnLocalizeCode { get; set; } = "";
        public string ColumnTitle { get; set; } = "";
        public string ColumnName { get; set; } = "";
        public bool HasSearch { get; set; } = false;
        public string SearchText { get; set; } = "";
        public SearchColumnType SearchType { get; set; } = SearchColumnType.Text;
        public Dictionary<string, string> OtherDropDownValues { get; set; }
        public bool HasSort { get; set; } = false;
        public GridViewSortStatus SortStatus { get; set; } = GridViewSortStatus.NoSort;
    }

}
