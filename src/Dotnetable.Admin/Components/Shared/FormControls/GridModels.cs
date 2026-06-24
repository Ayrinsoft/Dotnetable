using Dotnetable.Application.DTOs;

namespace Dotnetable.Admin.Components.Shared.FormControls;

/// <summary>Maps the grid's UI state into a server-side <see cref="GridQuery"/>.</summary>
public static class GridViewExtensions
{
    public static GridQuery ToQuery(this GridViewHeaderParameters p) => new()
    {
        PageIndex = p.Pagination.PageIndex,
        PageSize = p.Pagination.PageSize,
        OrderBy = string.IsNullOrWhiteSpace(p.OrderbyParams) ? null : p.OrderbyParams,
        Search = p.HeaderItems
            .Where(h => h.HasSearch && !string.IsNullOrWhiteSpace(h.SearchText))
            .ToDictionary(h => h.ColumnName, h => h.SearchText, StringComparer.OrdinalIgnoreCase),
    };
}

public enum GridViewSortStatus
{
    NoSort = 0,
    ASC,
    DESC
}

public enum SearchColumnType
{
    Text,
    CheckBox,
    Date,
    DropDown
}

/// <summary>State for the <see cref="DNPagination"/> control and server-side paging.</summary>
public class PaginationModel
{
    public int PageIndex { get; set; } = 1;
    public int MaxLength { get; set; } = 0;
    public int PageSize { get; set; } = 10;
    public bool ShowNumbers { get; set; } = true;
    public bool ShowFirstLast { get; set; } = true;
    public int VisiblePages { get; set; } = 5;
}

/// <summary>Header / sort / search / paging definition consumed by <see cref="DNGridView"/>.</summary>
public class GridViewHeaderParameters
{
    public List<HeaderItemDetail> HeaderItems { get; set; } = new();
    public string OrderbyParams { get; set; } = "";
    public bool HasRowNumber { get; set; } = false;
    public PaginationModel Pagination { get; set; } = new();

    public class HeaderItemDetail
    {
        public string ColumnLocalizeCode { get; set; } = "";
        public string ColumnTitle { get; set; } = "";
        public string ColumnName { get; set; } = "";
        public bool HasSearch { get; set; } = false;
        public string SearchText { get; set; } = "";
        public SearchColumnType SearchType { get; set; } = SearchColumnType.Text;
        public Dictionary<string, string>? OtherDropDownValues { get; set; }
        public bool HasSort { get; set; } = false;
        public GridViewSortStatus SortStatus { get; set; } = GridViewSortStatus.NoSort;
    }
}
