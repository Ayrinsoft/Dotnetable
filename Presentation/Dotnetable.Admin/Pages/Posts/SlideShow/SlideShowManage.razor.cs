using Dotnetable.Admin.Components.Posts.SlideShow;
using Dotnetable.Admin.Models;
using Dotnetable.Admin.Sahred.Dialogs;
using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.DTO.Post;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Pages.Posts.SlideShow;

public partial class SlideShowManage
{
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IStringLocalizer<Shared.Resources.Resource> _loc { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }
    [Inject] private IDialogService _dialogService { get; set; }
    [Inject] private IHttpContextAccessor _httpContextAccessor { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    private SlideShowListRequest _listRequest { get; set; }
    private SlideShowListResponse _listResponse { get; set; }
    private GridViewHeaderParameters _gridHeaderParams { get; set; }
    private HttpContext _context = null;

    protected async override Task OnInitializedAsync()
    {
        _context = _httpContextAccessor.HttpContext;
        _gridHeaderParams = new()
        {
            HeaderItems = new()
                {
                    new() { ColumnLocalizeCode = "_SlideShowID", ColumnName = nameof(SlideShowListResponse.SlideShowDetail.SlideShowID), HasSort = true },
                    new() { ColumnLocalizeCode = "_Title", ColumnName = nameof(SlideShowListResponse.SlideShowDetail.Title), HasSearch = true, SearchType = SearchColumnType.Text, HasSort = true },
                    new() { ColumnLocalizeCode = "_PageCode", ColumnName = nameof(SlideShowListResponse.SlideShowDetail.PageCode), HasSearch = true, SearchType = SearchColumnType.Text, HasSort = true },
                    new() { ColumnLocalizeCode = "_LanguageCode" },
                    new() { ColumnLocalizeCode = "_Priority" },
                    new() { ColumnLocalizeCode = "_FilePreview" },
                    new() { ColumnLocalizeCode = "_LanguageCodes" },
                    new() { ColumnLocalizeCode = "_Active_DeActive" },
                    new() { ColumnLocalizeCode = "_Management" }
                },
            Pagination = new() { MaxLength = _listResponse?.DatabaseRecords ?? 1, ShowFirstLast = true }
        };

        RefreshRequestInput();
        await FetchGrid();
    }

    #region GRID

    private async Task OnSearchChanged(GridViewHeaderParameters changedColumns)
    {
        _gridHeaderParams = changedColumns;
        RefreshRequestInput();
        await FetchGrid();
    }

    private void RefreshRequestInput()
    {
        _listRequest = new()
        {
            SkipCount = ((_gridHeaderParams.Pagination.PageIndex - 1) * _gridHeaderParams.Pagination.PageSize),
            TakeCount = _gridHeaderParams.Pagination.PageSize,
            OrderbyParams = _gridHeaderParams.OrderbyParams,
            Title = _gridHeaderParams.HeaderItems.FirstOrDefault(i => i.ColumnName == nameof(SlideShowListResponse.SlideShowDetail.Title))?.SearchText ?? "",
            PageCode = _gridHeaderParams.HeaderItems.FirstOrDefault(i => i.ColumnName == nameof(SlideShowListResponse.SlideShowDetail.PageCode))?.SearchText ?? ""
        };
    }

    private async Task FetchGrid()
    {
        var fetchPosts = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Post/SlideShowList", _listRequest.ToJsonString());
        if (fetchPosts.Success)
        {
            _listResponse = fetchPosts.ResponseData.CastModel<SlideShowListResponse>();
        }
        _gridHeaderParams.Pagination.MaxLength = _listResponse?.DatabaseRecords ?? 1;
        StateHasChanged();
    }
    #endregion


    #region CRUD

    private async Task InsertSlideShow()
    {
        var checkDialogData = await _dialogService.Show<SlideShowDialog>(_loc["_SlideShow_Insert"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true, FullWidth = true, FullScreen = true }, parameters: new() { { "FormModel", new SlideShowInsertRequest() } }).Result;
        if (checkDialogData.Canceled) return;

        var dialogresponseData = checkDialogData.Data.CastModel<SlideShowInsertRequest>();
        if (dialogresponseData is null) return;


        var serviceResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, $"Post/SlideShowInsert", dialogresponseData.ToJsonString());
        if (!serviceResponse.Success)
        {
            _snackbar.Add($"{_loc[$"_ERROR_{serviceResponse.ErrorException.ErrorCode}"]} {_loc["_SlideShow_Insert"]}", Severity.Error);
            return;
        }

        var parsedServiceResponse = serviceResponse.ResponseData.CastModel<PublicActionResponse>();
        if (parsedServiceResponse is null || !parsedServiceResponse.SuccessAction || parsedServiceResponse.ErrorException is not null)
        {
            _snackbar.Add($"{_loc[$"_ERROR_{serviceResponse.ErrorException.ErrorCode}"]} {_loc["_SlideShow_Insert"]}", Severity.Error);
            return;
        }

        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_SlideShow_Insert"]}", Severity.Success);

        _listResponse ??= new();
        _listResponse.SlideShows ??= new();
        int slideShowID = -1;
        if (int.TryParse(parsedServiceResponse.ObjectID, out int _slideShowID)) slideShowID = _slideShowID;
        Guid fileCode = Guid.NewGuid();
        if (Guid.TryParse(dialogresponseData.FileCode, out Guid _fileCode)) fileCode = _fileCode;

        _listResponse.SlideShows.Add(new() { Active = true, FileCode = fileCode, LanguageCode = themeManager.LanguageCode, LanguageCodes = "", PageCode = dialogresponseData.PageCode, Priority = dialogresponseData.Priority, SlideShowID = slideShowID, Title = dialogresponseData.Title });
    }


    private async Task ChangeActiveStatus(SlideShowListResponse.SlideShowDetail requestModel)
    {
        if ((await _dialogService.Show<ConfirmDialog>(_loc["_AreYouSure"], new DialogOptions { CloseOnEscapeKey = true, CloseButton = true, MaxWidth = MaxWidth.Small, Position = DialogPosition.Center }).Result).Canceled)
            return;

        var fetchResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Post/SlideShowChangeStatus", new SlideShowChangeStatusRequest { SlideShowID = requestModel.SlideShowID }.ToJsonString());
        if (!fetchResponse.Success)
        {
            _snackbar.Add($"{((fetchResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{fetchResponse.ErrorException?.ErrorCode}"])} {_loc["_Active_DeActive"]}", Severity.Error);
            return;
        }

        var parsedResponse = fetchResponse.ResponseData.CastModel<PublicActionResponse>();
        if (parsedResponse.SuccessAction)
        {
            requestModel.Active = !requestModel.Active;
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Active_DeActive"]}", Severity.Success);
        }
    }

    private async Task EditCurrentSlide(SlideShowListResponse.SlideShowDetail requestModel)
    {
        var fetchResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Post/SlideShowDetail", new SlideShowDetailRequest { SlideShowID = requestModel.SlideShowID }.ToJsonString());
        if (!fetchResponse.Success)
        {
            _snackbar.Add($"{((fetchResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{fetchResponse.ErrorException?.ErrorCode}"])} {_loc["_Active_DeActive"]}", Severity.Error);
            return;
        }

        var parsedResponse = fetchResponse.ResponseData.CastModel<SlideShowDetailResponse>();

        var checkDialogData = await _dialogService.Show<SlideShowDialog>(_loc["_SlideShow_Update"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true, FullWidth = true, FullScreen = true }, parameters: new() { { "FormModel", parsedResponse.CastModel<SlideShowInsertRequest>() } }).Result;
        if (checkDialogData.Canceled) return;

        var dialogresponseData = checkDialogData.Data.CastModel<SlideShowInsertRequest>();
        if (dialogresponseData is null) return;

        var serviceResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, $"Post/SlideShowUpdate", dialogresponseData.ToJsonString());
        if (!serviceResponse.Success)
        {
            _snackbar.Add($"{_loc[$"_ERROR_{serviceResponse.ErrorException.ErrorCode}"]} {_loc["_SlideShow_Update"]}", Severity.Error);
            return;
        }

        var parsedServiceResponse = serviceResponse.ResponseData.CastModel<PublicActionResponse>();
        if (parsedServiceResponse is null || !parsedServiceResponse.SuccessAction || parsedServiceResponse.ErrorException is not null)
        {
            _snackbar.Add($"{_loc[$"_ERROR_{serviceResponse.ErrorException.ErrorCode}"]} {_loc["_SlideShow_Update"]}", Severity.Error);
            return;
        }

        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_SlideShow_Update"]}", Severity.Success);

        requestModel.FileCode = new Guid(dialogresponseData.FileCode);
        requestModel.Title = dialogresponseData.Title;
        requestModel.PageCode = dialogresponseData.PageCode;
        requestModel.Priority = dialogresponseData.Priority;
    }

    private async Task<SlideShowInsertLanguageRequest> FetchSlideShowLanguageDetail(SlideShowLanguageDetailRequest requestModel)
    {
        var fetchResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Post/SlideShowLanguageDetail", requestModel.ToJsonString());
        if (fetchResponse.Success)
        {
            var parsedResponse = fetchResponse.ResponseData.CastModel<SlideShowInsertLanguageRequest>();
            if (parsedResponse is not null)
            {
                return parsedResponse;
            }
        }

        _snackbar.Add($"{((fetchResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{fetchResponse.ErrorException?.ErrorCode}"])} {_loc["_UpdateLanguage"]}", Severity.Warning);

        return null;
    }

    private async Task UpdateLanguage(SlideShowListResponse.SlideShowDetail requestModel)
    {
        var promptResponse = await _dialogService.Show<PromptDialog>(_loc["_AddNewLanguage"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true }, parameters: new() { { "ColumnTitle", (_loc["_Title"]).ToString() } }).Result;
        if (promptResponse.Canceled || promptResponse.Data.ToString() == "") return;

        var slideShowLanguageDetail = await FetchSlideShowLanguageDetail(new() { LanguageCode = promptResponse.Data.ToString().ToUpper(), SlideShowID = requestModel.SlideShowID });
        slideShowLanguageDetail ??= new() { SlideShowID = requestModel.SlideShowID, LanguageCode = promptResponse.Data.ToString().ToUpper() };

        var slideshowLanguage = await _dialogService.Show<SlideshowLanguageDialog>(_loc["_AddNewLanguage"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true }, parameters: new() { { "FormModel", slideShowLanguageDetail } }).Result;
        if (promptResponse.Canceled) return;

        var serviceResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, $"Post/SlideShowInsertLanguage", slideshowLanguage.Data.ToJsonString());
        if (serviceResponse.Success)
        {
            var parsedResponse = serviceResponse.ResponseData.CastModel<PublicActionResponse>();
            if (parsedResponse.SuccessAction)
                _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_SlideShow_Lanugage_Manage"]}", Severity.Success);
        }
        else
        {
            if (serviceResponse.ErrorException is not null && !string.IsNullOrEmpty(serviceResponse.ErrorException.ErrorCode))
                _snackbar.Add($"{_loc[$"_ERROR_{serviceResponse.ErrorException.ErrorCode}"]} {_loc["_SlideShow_Lanugage_Manage"]}", Severity.Error);
            else
                _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_SlideShow_Lanugage_Manage"]}", Severity.Error);
        }
    }

    private async Task DeleteLanguage(SlideShowListResponse.SlideShowDetail requestModel)
    {
        var promptResponse = await _dialogService.Show<PromptDialog>(_loc["_AddNewLanguage"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true }, parameters: new() { { "ColumnTitle", (_loc["_Title"]).ToString() } }).Result;
        if (promptResponse.Canceled || promptResponse.Data.ToString() == "") return;


        var fetchResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Post/SlideShowRemoveLanguage", new SlideShowRemoveLanguageRequest { SlideShowID = requestModel.SlideShowID, LanguageCode = promptResponse.Data.ToString() }.ToJsonString());
        if (!fetchResponse.Success)
        {
            _snackbar.Add($"{((fetchResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{fetchResponse.ErrorException?.ErrorCode}"])} {_loc["_DeleteLanguage"]}", Severity.Error);
            return;
        }

        var parsedResponse = fetchResponse.ResponseData.CastModel<PublicActionResponse>();
        if (parsedResponse.SuccessAction)
        {
            var fetchUpdatedPost = (from i in _listResponse.SlideShows where i.SlideShowID == requestModel.SlideShowID select i).FirstOrDefault();
            if (fetchUpdatedPost is not null && !string.IsNullOrEmpty(fetchUpdatedPost.LanguageCodes) && fetchUpdatedPost.LanguageCodes != "")
            {
                fetchUpdatedPost.LanguageCodes = fetchUpdatedPost.LanguageCodes.Replace(promptResponse.Data.ToString(), "", StringComparison.OrdinalIgnoreCase);
            }
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_DeleteLanguage"]}", Severity.Success);

        }
    }


    #endregion


}
