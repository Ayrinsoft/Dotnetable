using Dotnetable.Shared.DTO.Public;
using Microsoft.AspNetCore.Components;

namespace Dotnetable.Admin.Components.Shared.FormControls;

public partial class DNPagination
{
    [Parameter] public PaginationModel PaginationModel { get; set; }
    [Parameter] public EventCallback<PaginationModel> OnPageChanged { get; set; }


    private readonly int[] _paginationSize = new int[] { 10, 25, 50, 100, 200 };
    private int _pageCount { get; set; }
    private int _adjusted { get; set; }
    private int _half { get; set; }
    private int _start { get; set; }
    private int _finish { get; set; }
    private bool _hasPrevios { get; set; }
    private bool _hasNext { get; set; }

    protected override void OnInitialized()
    {
        CheckItems();
    }

    private async Task PagerButtonClicked(int page)
    {
        if (page < 1 || page > _pageCount) return;
        PaginationModel.PageIndex = page;
        CheckItems();
        await OnPageChanged.InvokeAsync(PaginationModel);
        StateHasChanged();
    }

    private void CheckItems()
    {
        _pageCount = PaginationModel.MaxLength / PaginationModel.PageSize + (PaginationModel.MaxLength - PaginationModel.MaxLength / PaginationModel.PageSize * PaginationModel.PageSize > 0 ? 1 : 0);
        if (_pageCount < PaginationModel.VisiblePages) PaginationModel.VisiblePages = _pageCount;
        if (_pageCount > PaginationModel.VisiblePages && PaginationModel.VisiblePages < 6) PaginationModel.VisiblePages = _pageCount > 6 ? 6 : _pageCount;
        if (_pageCount <= 5) PaginationModel.ShowFirstLast = false;
        _adjusted = Math.Min(PaginationModel.VisiblePages, _pageCount);
        _half = (int)Math.Floor(_adjusted / 2d);
        _start = Math.Max(PaginationModel.PageIndex - _half, 1);
        _finish = Math.Min(PaginationModel.PageIndex + _half, _pageCount);
        if (_start <= 1) { _start = 1; _finish = _adjusted; }
        if (_finish >= _pageCount) { _start = _pageCount - _adjusted; }
        if (_start <= 1) { _start = 1; }
        _hasPrevios = PaginationModel.PageIndex > 1 && _pageCount > 1;
        _hasNext = PaginationModel.PageIndex < _pageCount;
        StateHasChanged();
    }

    private async Task OnChangePageSize(ChangeEventArgs e)
    {
        PaginationModel.PageSize = Convert.ToInt32(e.Value);
        CheckItems();
        await OnPageChanged.InvokeAsync(PaginationModel);
        StateHasChanged();
    }
}
