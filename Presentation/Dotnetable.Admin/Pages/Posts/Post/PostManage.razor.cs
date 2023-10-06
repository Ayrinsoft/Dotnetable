using Dotnetable.Admin.Components.Posts.Post;
using Dotnetable.Admin.Models;
using Dotnetable.Admin.Sahred.Dialogs;
using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.DTO.Post;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Pages.Posts.Post;

public partial class PostManage
{
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IStringLocalizer<Shared.Resources.Resource> _loc { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }
    [Inject] private IDialogService _dialogService { get; set; }
    [Inject] private IMemoryCache _mmc { get; set; }
    [Inject] private IConfiguration _config { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    private List<PostCategoryListResponse.PostCategoryDetail> _cachedPostCategory { get; set; }
    private PostListFetchRequest _postListRequest { get; set; }
    private PostListFetchResponse _postListResponse { get; set; }
    private GridViewHeaderParameters _gridHeaderParams { get; set; }
    private string _defaultlanguageCode;

    protected async override Task OnInitializedAsync()
    {
        if (!_mmc.TryGetValue("PostCategoryList", out List<PostCategoryListResponse.PostCategoryDetail> _postCategoryList))
        {
            var fetchServiceData = await _httpService.CallServiceObjAsync(HttpMethod.Get, true, "Post/PostCategoryList");
            if (fetchServiceData.Success)
            {
                _postCategoryList = fetchServiceData.ResponseData.CastModel<PostCategoryListResponse>().PostCategories;
                _mmc.Set("PostCategoryList", _postCategoryList, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(3)));
            }
        }
        _cachedPostCategory = _postCategoryList;

        _defaultlanguageCode = _config["AdminPanelSettings:DefaultLanguageCode"];

        _gridHeaderParams = new()
        {
            HeaderItems = new()
                {
                    new() { ColumnLocalizeCode = "_PostID", ColumnName = nameof(PostListFetchResponse.PostDetail.PostID), HasSort = true },
                    new() { ColumnLocalizeCode = "_PostCategoryName" },
                    new() { ColumnLocalizeCode = "_ModifierName" },
                    new() { ColumnLocalizeCode = "_ModifyDate" },
                    new() { ColumnLocalizeCode = "_Title", ColumnName = nameof(PostListFetchResponse.PostDetail.Title), HasSearch = true, SearchType = SearchColumnType.Text, HasSort = true },
                    new() { ColumnLocalizeCode = "_LanguageCodes" },
                    new() { ColumnLocalizeCode = "_Active_DeActive" },
                    new() { ColumnLocalizeCode = "_Management" }
                },
            Pagination = new() { MaxLength = _postListResponse?.DatabaseRecords ?? 1, ShowFirstLast = true }
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
        _postListRequest = new()
        {
            SkipCount = ((_gridHeaderParams.Pagination.PageIndex - 1) * _gridHeaderParams.Pagination.PageSize),
            TakeCount = _gridHeaderParams.Pagination.PageSize,
            OrderbyParams = _gridHeaderParams.OrderbyParams,
            Title = _gridHeaderParams.HeaderItems.FirstOrDefault(i => i.ColumnName == nameof(PostListFetchResponse.PostDetail.Title))?.SearchText ?? ""
        };
    }

    private async Task FetchGrid()
    {
        var fetchPosts = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Post/AdminPostList", _postListRequest.ToJsonString());
        if (fetchPosts.Success)
        {
            _postListResponse = fetchPosts.ResponseData.CastModel<PostListFetchResponse>();
        }
        _gridHeaderParams.Pagination.MaxLength = _postListResponse?.DatabaseRecords ?? 1;
        StateHasChanged();
    }
    #endregion

    #region CRUD

    private async Task InsertNewPost()
    {
        var checkDialogData = await _dialogService.Show<PostDialog>(_loc["_Post_Insert"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true, FullWidth = true, FullScreen = true }, parameters: new() { { "FormModel", new PostUpdateRequest() }, { "FunctionName", "Insert" } }).Result;
        if (checkDialogData.Canceled) return;

        var dialogresponseData = checkDialogData.Data.CastModel<PostUpdateRequest>();
        if (dialogresponseData is null) return;

        var updateOnAPIResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Post/Insert", dialogresponseData.ToJsonString());
        if (!updateOnAPIResponse.Success)
        {
            _snackbar.Add($"{((updateOnAPIResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{updateOnAPIResponse.ErrorException?.ErrorCode}"])} {_loc["_Post_Insert"]}", Severity.Error);
            return;
        }

        var parsedResponse = updateOnAPIResponse.ResponseData.CastModel<PublicActionResponse>();
        if (!parsedResponse.SuccessAction)
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Post_Insert"]}", Severity.Error);

        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Post_Insert"]}", Severity.Success);
    }

    private async Task ChangeActiveStatus(PostListFetchResponse.PostDetail requestModel)
    {
        var fetchResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Post/ChangeStatus", new PostChangeStatusRequest { PostID = requestModel.PostID }.ToJsonString());
        if (!fetchResponse.Success)
        {
            _snackbar.Add($"{((fetchResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{fetchResponse.ErrorException?.ErrorCode}"])} {_loc["_Active_DeActive"]}", Severity.Error);
            return;
        }

        var parsedResponse = fetchResponse.ResponseData.CastModel<PublicActionResponse>();
        if (!parsedResponse.SuccessAction) return;

        requestModel.Active = !requestModel.Active;
        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Active_DeActive"]}", Severity.Success);
    }

    private async Task EditCurrentPost(PostListFetchResponse.PostDetail requestModel)
    {
        var fetchResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Post/AdminDetail", new PostDetailRequest { PostID = requestModel.PostID }.ToJsonString());
        if (!fetchResponse.Success)
        {
            _snackbar.Add($"{((fetchResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{fetchResponse.ErrorException?.ErrorCode}"])} {_loc["_Edit"]}", Severity.Error);
            return;
        }
        var postDetailAdmin = fetchResponse.ResponseData.CastModel<PostDetailResponse>();
        var editDataModel = postDetailAdmin.CastModel<PostUpdateRequest>();
        editDataModel.FileCodes = postDetailAdmin.FileList.Select(i => i.FileCode.ToString()).ToList();

        var checkDialogData = await _dialogService.Show<PostDialog>(_loc["_Post_Update"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true, FullWidth = true, FullScreen = true }, parameters: new() { { "FormModel", editDataModel }, { "FunctionName", "Update" } }).Result;
        if (checkDialogData.Canceled) return;

        var dialogresponseData = checkDialogData.Data.CastModel<PostUpdateRequest>();
        if (dialogresponseData is null) return;

        dialogresponseData.PostID = requestModel.PostID;

        var updateOnAPIResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Post/Update", dialogresponseData.ToJsonString());
        if (!updateOnAPIResponse.Success)
        {
            _snackbar.Add($"{((updateOnAPIResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{updateOnAPIResponse.ErrorException?.ErrorCode}"])} {_loc["_Post_Update"]}", Severity.Error);
            return;
        }

        var parsedResponse = updateOnAPIResponse.ResponseData.CastModel<PublicActionResponse>();
        if (!parsedResponse.SuccessAction) return;

        try
        {
            requestModel.MetaDescription = dialogresponseData.MetaDescription;
            requestModel.MetaKeywords = dialogresponseData.MetaKeywords;
            requestModel.ModifyDate = DateTime.Now;
            requestModel.Summary = dialogresponseData.Summary;
            requestModel.Title = dialogresponseData.Title;
            requestModel.Tags = dialogresponseData.Tags;
            requestModel.PostCategoryName = _cachedPostCategory?.FirstOrDefault(i => i.PostCategoryID == dialogresponseData.PostCategoryID)?.Title ?? "";
        }
        catch (Exception) { }

        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Post_Update"]}", Severity.Success);
    }

    private async Task DeleteLanguage(PostListFetchResponse.PostDetail requestModel)
    {
        if ((await _dialogService.Show<ConfirmDialog>(_loc["_AreYouSure"], new DialogOptions { CloseOnEscapeKey = true, CloseButton = true, MaxWidth = MaxWidth.Small, Position = DialogPosition.Center }).Result).Canceled)
            return;

        var promptResponse = await _dialogService.Show<PromptDialog>(_loc["_DeleteLanguage"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true }, parameters: new() { { "ColumnTitle", (_loc["_LanguageCode"]).ToString() } }).Result;
        if (promptResponse.Canceled || promptResponse.Data.ToString() == "") return;

        var fetchResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Post/PostDeleteLangauge", new PostLanguageDeleteRequest { PostID = requestModel.PostID, LanguageCode = promptResponse.Data.ToString() }.ToJsonString());
        if (!fetchResponse.Success)
        {
            _snackbar.Add($"{((fetchResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{fetchResponse.ErrorException?.ErrorCode}"])} {_loc["_DeleteLanguage"]}", Severity.Error);
            return;
        }

        var parsedResponse = fetchResponse.ResponseData.CastModel<PublicActionResponse>();
        if (parsedResponse.SuccessAction)
        {
            requestModel.LanguageCodes ??= "";
            requestModel.LanguageCodes = requestModel.LanguageCodes.Replace(promptResponse.Data.ToString(), "", StringComparison.OrdinalIgnoreCase);
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_DeleteLanguage"]}", Severity.Success);
        }
        else
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_DeleteLanguage"]}", Severity.Error);
        }
    }

    private async Task UpdateLanguage(PostListFetchResponse.PostDetail requestModel)
    {
        var promptResponse = await _dialogService.Show<PromptDialog>(_loc["_Add_Language"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true }, parameters: new() { { "ColumnTitle", (_loc["_LanguageCode"]).ToString() } }).Result;
        if (promptResponse.Canceled || promptResponse.Data.ToString() == "") return;

        PostUpdateRequest editDataModel = new() { PostID = requestModel.PostID, LanguageCode = promptResponse.Data.ToString().ToUpper(), PostCategoryID = requestModel.PostCategoryID };

        var fetchResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Post/PostLanguageDetail", new PostLanguageDetailRequest { PostID = requestModel.PostID, LanguageCode = editDataModel.LanguageCode }.ToJsonString());
        if (fetchResponse is not null && fetchResponse.Success)
        {
            var tempEditModel = fetchResponse.ResponseData.CastModel<PostUpdateRequest>();
            if (tempEditModel is not null) editDataModel = tempEditModel;
        }

        var checkDialogData = await _dialogService.Show<PostDialog>(_loc["_Post_Update"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true, FullWidth = true, FullScreen = true }, parameters: new() { { "FormModel", editDataModel }, { "FunctionName", "UpdateLanguage" }, { "DefaultLanguageCode", editDataModel.LanguageCode } }).Result;
        if (checkDialogData.Canceled) return;

        var dialogresponseData = checkDialogData.Data.CastModel<PostUpdateRequest>();
        if (dialogresponseData is null) return;

        dialogresponseData.PostID = requestModel.PostID;

        var updateOnAPIResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, $"Post/PostAddLanguage", dialogresponseData.ToJsonString());
        if (!updateOnAPIResponse.Success)
        {
            _snackbar.Add($"{((updateOnAPIResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{updateOnAPIResponse.ErrorException?.ErrorCode}"])} {_loc["_Active_DeActive"]}", Severity.Error);
            return;
        }

        var parsedResponse = updateOnAPIResponse.ResponseData.CastModel<PublicActionResponse>();
        if (!parsedResponse.SuccessAction) return;

        requestModel.LanguageCodes ??= "";
        if (!requestModel.LanguageCodes.Contains(dialogresponseData.LanguageCode))
            requestModel.LanguageCodes += $", {dialogresponseData.LanguageCode}";

        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Post_Update"]}", Severity.Success);
    }

    #endregion

}
