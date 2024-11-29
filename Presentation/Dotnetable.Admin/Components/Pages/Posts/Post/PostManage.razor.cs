using Dotnetable.Admin.Components.PageComponents.Posts.Post;
using Dotnetable.Admin.Components.Shared.Dialogs;
using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Post;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.Pages.Posts.Post;

public partial class PostManage
{
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private PostService _post { get; set; }
    [Inject] private IDialogService _dialogService { get; set; }
    [Inject] private IMemoryCache _mmc { get; set; }
    [Inject] private IConfiguration _config { get; set; }
    [Inject] private Tools _tools { get; set; }
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
            int currentMemberID = await _tools.GetRequesterMemberID();
            var fetchServiceData = await _post.PostCategoryList(currentMemberID);
            if (fetchServiceData.ErrorException is null)
            {
                _postCategoryList = fetchServiceData.PostCategories;
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
        var fetchPosts = await _post.AdminPostList(_postListRequest);
        if (fetchPosts.ErrorException is null)
        {
            _postListResponse = fetchPosts;
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

        var updateOnAPIResponse = await _post.Insert(dialogresponseData);
        if (!updateOnAPIResponse.SuccessAction)
        {
            _snackbar.Add($"{((updateOnAPIResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{updateOnAPIResponse.ErrorException?.ErrorCode}"])} {_loc["_Post_Insert"]}", Severity.Error);
            return;
        }

        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Post_Insert"]}", Severity.Success);
    }

    private async Task ChangeActiveStatus(PostListFetchResponse.PostDetail requestModel)
    {
        var fetchResponse = await _post.ChangeStatus(new() { PostID = requestModel.PostID });
        if (!fetchResponse.SuccessAction)
        {
            _snackbar.Add($"{((fetchResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{fetchResponse.ErrorException?.ErrorCode}"])} {_loc["_Active_DeActive"]}", Severity.Error);
            return;
        }

        requestModel.Active = !requestModel.Active;
        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Active_DeActive"]}", Severity.Success);
    }

    private async Task EditCurrentPost(PostListFetchResponse.PostDetail requestModel)
    {
        var fetchResponse = await _post.AdminDetail(new() { PostID = requestModel.PostID });
        if (fetchResponse.ErrorException is not null)
        {
            _snackbar.Add($"{((fetchResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{fetchResponse.ErrorException?.ErrorCode}"])} {_loc["_Edit"]}", Severity.Error);
            return;
        }

        var editDataModel = fetchResponse.CastModel<PostUpdateRequest>();
        editDataModel.FileCodes = fetchResponse.FileList.Select(i => i.FileCode.ToString()).ToList();

        var checkDialogData = await _dialogService.Show<PostDialog>(_loc["_Post_Update"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true, FullWidth = true, FullScreen = true }, parameters: new() { { "FormModel", editDataModel }, { "FunctionName", "Update" } }).Result;
        if (checkDialogData.Canceled) return;

        var dialogresponseData = checkDialogData.Data.CastModel<PostUpdateRequest>();
        if (dialogresponseData is null) return;

        dialogresponseData.PostID = requestModel.PostID;

        var updateOnAPIResponse = await _post.Update(dialogresponseData);
        if (!updateOnAPIResponse.SuccessAction)
        {
            _snackbar.Add($"{((updateOnAPIResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{updateOnAPIResponse.ErrorException?.ErrorCode}"])} {_loc["_Post_Update"]}", Severity.Error);
            return;
        }

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

        var fetchResponse = await _post.PostDeleteLangauge(new() { PostID = requestModel.PostID, LanguageCode = promptResponse.Data.ToString() });
        if (!fetchResponse.SuccessAction)
        {
            _snackbar.Add($"{((fetchResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{fetchResponse.ErrorException?.ErrorCode}"])} {_loc["_DeleteLanguage"]}", Severity.Error);
            return;
        }

        requestModel.LanguageCodes ??= "";
        requestModel.LanguageCodes = requestModel.LanguageCodes.Replace(promptResponse.Data.ToString(), "", StringComparison.OrdinalIgnoreCase);
        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_DeleteLanguage"]}", Severity.Success);
    }

    private async Task UpdateLanguage(PostListFetchResponse.PostDetail requestModel)
    {
        var promptResponse = await _dialogService.Show<PromptDialog>(_loc["_Add_Language"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true }, parameters: new() { { "ColumnTitle", (_loc["_LanguageCode"]).ToString() } }).Result;
        if (promptResponse.Canceled || promptResponse.Data.ToString() == "") return;

        PostUpdateRequest editDataModel = new() { PostID = requestModel.PostID, LanguageCode = promptResponse.Data.ToString().ToUpper(), PostCategoryID = requestModel.PostCategoryID };

        var fetchResponse = await _post.PostLanguageDetail(new() { PostID = requestModel.PostID, LanguageCode = editDataModel.LanguageCode });
        if (fetchResponse is not null && fetchResponse.ErrorException is null)
        {
            var tempEditModel = fetchResponse.CastModel<PostUpdateRequest>();
            if (tempEditModel is not null) editDataModel = tempEditModel;
        }

        var checkDialogData = await _dialogService.Show<PostDialog>(_loc["_Post_Update"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true, FullWidth = true, FullScreen = true }, parameters: new() { { "FormModel", editDataModel }, { "FunctionName", "UpdateLanguage" }, { "DefaultLanguageCode", editDataModel.LanguageCode } }).Result;
        if (checkDialogData.Canceled) return;

        var dialogresponseData = checkDialogData.Data.CastModel<PostUpdateRequest>();
        if (dialogresponseData is null) return;

        dialogresponseData.PostID = requestModel.PostID;

        var updateOnAPIResponse = await _post.PostAddLanguage(dialogresponseData);
        if (!updateOnAPIResponse.SuccessAction)
        {
            _snackbar.Add($"{((updateOnAPIResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{updateOnAPIResponse.ErrorException?.ErrorCode}"])} {_loc["_Active_DeActive"]}", Severity.Error);
            return;
        }

        requestModel.LanguageCodes ??= "";
        if (!requestModel.LanguageCodes.Contains(dialogresponseData.LanguageCode))
            requestModel.LanguageCodes += $", {dialogresponseData.LanguageCode}";

        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Post_Update"]}", Severity.Success);
    }

    #endregion

}
