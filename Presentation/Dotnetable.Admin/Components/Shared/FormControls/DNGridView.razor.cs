using Dotnetable.Admin.Models;
using Dotnetable.Shared.DTO.Public;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;

namespace Dotnetable.Admin.Components.Shared.FormControls;

public partial class DNGridView
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }

    [Parameter] public RenderFragment ChildContent { get; set; }
    [Parameter] public GridViewHeaderParameters GridHeaderParams { get; set; }
    [Parameter] public EventCallback<GridViewHeaderParameters> OnSearchChanged { get; set; }

    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    private bool _showGridViewSearchBar = false;

    private async Task OnPaginationChanged(PaginationModel pageChange)
    {
        GridHeaderParams.Pagination = pageChange;
        await OnSearchChanged.InvokeAsync(GridHeaderParams);
    }

    private void ToggleSearchBar()
    {
        _showGridViewSearchBar = !_showGridViewSearchBar;
    }

    private async Task SearchChanged(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await OnSearchChanged.InvokeAsync(GridHeaderParams);
        }
    }

    private async Task SortChanged(string columnName)
    {
        var fetchSortItem = (from i in GridHeaderParams.HeaderItems where i.ColumnName == columnName select i).FirstOrDefault();
        if (fetchSortItem.SortStatus == GridViewSortStatus.DESC)
            fetchSortItem.SortStatus = GridViewSortStatus.ASC;
        else
            fetchSortItem.SortStatus = GridViewSortStatus.DESC;

        string orderbyParams = $"{columnName} {(fetchSortItem.SortStatus == GridViewSortStatus.DESC ? "DESC" : "ASC")}";
        if (GridHeaderParams.HeaderItems.Any(i => i.HasSort && i.SortStatus != GridViewSortStatus.NoSort && i.ColumnName != columnName))
        {
            foreach (var j in GridHeaderParams.HeaderItems.Where(i => i.HasSort && i.SortStatus != GridViewSortStatus.NoSort && i.ColumnName != columnName).ToList())
            {
                orderbyParams += $", {j.ColumnName} {(j.SortStatus == GridViewSortStatus.DESC ? "DESC" : "ASC")}";
            }
        }
        GridHeaderParams.OrderbyParams = orderbyParams;

        await OnSearchChanged.InvokeAsync(GridHeaderParams);
    }

    private async Task OnChangeSearchDropdown(ChangeEventArgs e, string columnName)
    {
        string dropdownChangeValue = e.Value.ToString();
        var fetchParam = (from i in GridHeaderParams.HeaderItems where i.ColumnName == columnName select i).FirstOrDefault();
        fetchParam.SearchText = dropdownChangeValue == "X_SELECT_ITEM_X" ? "" : dropdownChangeValue;
        await OnSearchChanged.InvokeAsync(GridHeaderParams);
    }

    private async Task OnChangeSearchCheckBox(ChangeEventArgs e, string columnName)
    {
        var fetchParam = (from i in GridHeaderParams.HeaderItems where i.ColumnName == columnName select i).FirstOrDefault();
        fetchParam.SearchText = e.Value.ToString();
        await OnSearchChanged.InvokeAsync(GridHeaderParams);
    }
}
