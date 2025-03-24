using BlazorContextMenu;
using Dotnetable.Admin.Components.PageComponents.Posts.PostCategory;
using Dotnetable.Admin.Components.Shared.Dialogs;
using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.DTO.Post;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.Pages.Posts.PostCategory;

public partial class PostCategoryManage
{
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }
    [Inject] private IDialogService _dialogService { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    private List<PostCategoryListResponse.PostCategoryDetail> _cachedPostCategory { get; set; }
    private List<Models.Nestable.NestableStandardRequest> _nestableItems = new();
    private List<Models.Nestable.NestableStandardResponse> _updatedListItems = null;
    
    protected async override Task OnInitializedAsync()
    {
        var fetchPostCategory = await _httpService.CallServiceObjAsync(HttpMethod.Get, true, "Post/PostCategoryList");
        if (fetchPostCategory.Success)
        {
            _cachedPostCategory = fetchPostCategory.ResponseData.CastModel<PostCategoryListResponse>().PostCategories;
            UpdateNestableItems();
        }
    }

    private void UpdateNestableItems()
    {
        if (_cachedPostCategory != null)
        {
            foreach (var j in _cachedPostCategory.Where(x => x.ParentID == 0)) j.ParentID = null;

            var aggregateLanguages = (from i in _cachedPostCategory where i.LanguageCode == themeManager.LanguageCode select new { i.PostCategoryID, Languages = (from j in _cachedPostCategory.DefaultIfEmpty() where j.PostCategoryID == i.PostCategoryID && j.LanguageCode != themeManager.LanguageCode select j.LanguageCode) }).ToList();
            var aggregateLanguagesFinal = (from i in aggregateLanguages select new { i.PostCategoryID, LanguageCodes = string.Join(", ", i.Languages.Select(x => x)) }).ToList();

            _nestableItems = (from i in _cachedPostCategory where i.LanguageCode == themeManager.LanguageCode select new Models.Nestable.NestableStandardRequest { ItemID = i.PostCategoryID.ToString(), ParentID = i.ParentID.HasValue ? i.ParentID.ToString() : string.Empty, Priority = i.Priority, Title = $"{i.Title} - [{(aggregateLanguagesFinal.FirstOrDefault(j => j.PostCategoryID == i.PostCategoryID)?.LanguageCodes ?? "")}]", Active = i.Active }).ToList();
        }
    }


    private void NestableChanges(List<Models.Nestable.NestableStandardResponse> responseItems) => _updatedListItems = responseItems;

    #region CRUD

    private async Task UpdatePriorityChanges()
    {
        if (_updatedListItems is null) return;
        var requestModel = (from i in _updatedListItems select new PostCategoryUpdatePriorityAndParentRequest() { ParentID = Convert.ToInt32(i.ParentID), PostCategoryID = Convert.ToInt32(i.ItemID), Priority = (short)i.Priority }).ToList();
        if (requestModel is null) return;

        var serviceResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, $"Post/PostCategoryUpdatePriorityAndParent", requestModel.ToJsonString());
        if (serviceResponse.Success)
        {
            var parsedResponse = serviceResponse.ResponseData.CastModel<Dotnetable.Shared.DTO.Public.PublicActionResponse>();
            if (parsedResponse.SuccessAction)
            {
                _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_PostCategoy_ChangePriority_Update"]}", Severity.Success);

                Parallel.ForEach(_cachedPostCategory, j =>
                {
                    var fetchCurrentItem = (from i in _updatedListItems where i.ItemID == j.PostCategoryID.ToString() select i).FirstOrDefault();
                    if (fetchCurrentItem is null) return;

                    j.ParentID = string.IsNullOrEmpty(fetchCurrentItem.ParentID) || fetchCurrentItem.ParentID == "0" ? null : (int?)(Convert.ToInt32(fetchCurrentItem.ParentID));
                    j.Priority = (short)fetchCurrentItem.Priority;
                });
                _updatedListItems = null;
            }
            else if (parsedResponse.ErrorException != null && !string.IsNullOrEmpty(parsedResponse.ErrorException.ErrorCode))
            {
                _snackbar.Add($"{_loc[$"_ERROR_{parsedResponse.ErrorException.ErrorCode}"]} {_loc["_PostCategoy_ChangePriority_Update"]}", Severity.Error);
            }
        }
        else
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_PostCategoy_ChangePriority_Update"]}", Severity.Error);
        }
    }

    private async Task PostCategoryInsert(int? parentID = null)
    {
        var checkDialogData = await (await _dialogService.ShowAsync<PostCategoryDialog>(_loc["_PostCategory_Insert"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true, FullWidth = true }, parameters: new() { { "DefaultLanguageCode", themeManager.LanguageCode } })).Result;
        if (checkDialogData.Canceled) return;

        var dialogresponseData = checkDialogData.Data.CastModel<PostCategoryInsertRequest>();
        if (dialogresponseData is null) return;
        dialogresponseData.ParentID = parentID;

        var serviceResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, $"Post/PostCategoryInsert", dialogresponseData.ToJsonString());
        if (!serviceResponse.Success)
        {
            string errorCode = (serviceResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{serviceResponse.ErrorException?.ErrorCode}"];
            _snackbar.Add($"{errorCode} {_loc["_PostCategory_Insert"]}", Severity.Error);
            return;
        }

        var parsedResponse = serviceResponse.ResponseData.CastModel<Dotnetable.Shared.DTO.Public.PublicActionResponse>();
        if (parsedResponse.SuccessAction)
        {
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_PostCategory_Insert"]}", Severity.Success);
            dialogresponseData.PostCategoryID = Convert.ToInt32(parsedResponse.ObjectID);
            _cachedPostCategory.Add(new() { Active = true, LanguageCode = themeManager.LanguageCode, ParentID = dialogresponseData.ParentID, PostCategoryID = dialogresponseData.PostCategoryID.Value, Priority = (dialogresponseData.Priority ?? 0), Title = dialogresponseData.Title });
            UpdateNestableItems();
        }
    }

    private async Task EditItem(ItemClickEventArgs e)
    {
        var updateModel = _cachedPostCategory.FirstOrDefault(i => i.PostCategoryID == Convert.ToInt32(e.Data));
        if (updateModel is null) return;

        var serviceResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, $"Post/PostCategoryDetail", new PostCategoryDetailRequest() { PostCategoryID = updateModel.PostCategoryID, LanguageCode = updateModel.LanguageCode }.ToJsonString());
        if (!serviceResponse.Success) return;

        var parsedResponse = serviceResponse.ResponseData.CastModel<PostCategoryInsertRequest>();
        var checkDialogData = await (await _dialogService.ShowAsync<PostCategoryDialog>(_loc["_SlideShow_Update"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true, FullWidth = true }, parameters: new() { { "FormModel", parsedResponse }, { "DefaultLanguageCode", "EN" } })).Result;
        if (checkDialogData.Canceled) return;

        var dialogresponseData = checkDialogData.Data.CastModel<PostCategoryInsertRequest>();
        if (dialogresponseData is null) return;

        var updateServiceResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, $"Post/PostCategoryUpdate", dialogresponseData.ToJsonString());
        if (!updateServiceResponse.Success)
        {
            string errorCode = (updateServiceResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{updateServiceResponse.ErrorException?.ErrorCode}"];
            _snackbar.Add($"{errorCode} {_loc["_PostCategory_Insert"]}", Severity.Error);
            return;
        }

        var updateparsedResponse = updateServiceResponse.ResponseData.CastModel<Dotnetable.Shared.DTO.Public.PublicActionResponse>();
        if (updateparsedResponse.SuccessAction)
        {
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_PostCategory_Insert"]}", Severity.Success);
            var fetchCacheItem = (from i in _cachedPostCategory where i.PostCategoryID == Convert.ToInt32(updateparsedResponse.ObjectID) select i).FirstOrDefault();
            if (fetchCacheItem is not null)
            {
                fetchCacheItem.LanguageCode = themeManager.LanguageCode;
                fetchCacheItem.Title = dialogresponseData.Title;
            }
            UpdateNestableItems();
        }
    }

    private async Task AddChildItem(ItemClickEventArgs e) => await PostCategoryInsert(Convert.ToInt32(e.Data));

    private async Task ChangeStatus(ItemClickEventArgs e)
    {
        var updateModel = _cachedPostCategory.FirstOrDefault(i => i.PostCategoryID == Convert.ToInt32(e.Data));
        if (updateModel is null) return;

        var serviceResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, $"Post/PostCategoryChangeStatus", new PostCategoryChangeStatusRequest() { PostCategoryID = Convert.ToInt32(e.Data) }.ToJsonString());
        if (!serviceResponse.Success)
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_PostCategory_ChangeStatus"]}", Severity.Error);
            return;
        }

        var parsedResponse = serviceResponse.ResponseData.CastModel<Dotnetable.Shared.DTO.Public.PublicActionResponse>();
        if (parsedResponse.SuccessAction)
        {
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_PostCategory_ChangeStatus"]}", Severity.Success);

            updateModel.Active = !updateModel.Active;
            var Nitem = (from i in _nestableItems where i.ItemID == e.Data.ToString() select i).FirstOrDefault();
            if (Nitem != null) Nitem.Active = !Nitem.Active;

            StateHasChanged();
        }
        else if (parsedResponse.ErrorException != null && !string.IsNullOrEmpty(parsedResponse.ErrorException.ErrorCode))
        {
            _snackbar.Add($"{_loc[$"_ERROR_{parsedResponse.ErrorException.ErrorCode}"]} {_loc["_PostCategory_ChangeStatus"]}", Severity.Error);
        }

    }

    private async Task AppendLanguage(ItemClickEventArgs e)
    {
        var promptResponse = await (await _dialogService.ShowAsync<PromptDialog>(_loc["_AddNewLanguage"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true }, parameters: new() { { "ColumnTitle", (_loc["_LanguageCode"]).ToString() } })).Result;
        if (promptResponse.Canceled || promptResponse.Data.ToString() == "") return;
        int postCategoryID = Convert.ToInt32(e.Data);

        var updateModel = _cachedPostCategory.FirstOrDefault(i => i.PostCategoryID == postCategoryID);
        if (updateModel is null) return;

        var postCategoryDetailResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, $"Post/PostCategoryDetail", new PostCategoryDetailRequest() { PostCategoryID = postCategoryID, LanguageCode = promptResponse.Data.ToString() }.ToJsonString());

        PostCategoryUpdateOtherLanguageRequest formModel = new() { PostCategoryID = postCategoryID, LanguageCode = promptResponse.Data.ToString() };
        if (postCategoryDetailResponse.Success)
        {
            var postCategoryDetail = postCategoryDetailResponse.ResponseData.CastModel<PostCategoryDetailResponse>();
            if (postCategoryDetail != null)
                formModel = new() { LanguageCode = postCategoryDetail.LanguageCode, Description = postCategoryDetail.Description, MetaDescription = postCategoryDetail.MetaDescription, MetaKeywords = postCategoryDetail.MetaKeywords, PostCategoryID = postCategoryDetail.PostCategoryID, Tags = postCategoryDetail.Tags, Title = postCategoryDetail.Title };
        }

        var checkDialogData = await (await _dialogService.ShowAsync<PostCategoryLanguageDialog>(_loc["_PostCategory_Language"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true, FullWidth = true }, parameters: new() { { "FormModel", formModel } })).Result;
        if (checkDialogData.Canceled) return;

        var dialogresponseData = checkDialogData.Data.CastModel<PostCategoryUpdateOtherLanguageRequest>();
        if (dialogresponseData is null) return;
        dialogresponseData.PostCategoryID = postCategoryID;

        var serviceResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, $"Post/PostCategoryUpdateOtherLanguage", dialogresponseData.ToJsonString());
        if (!serviceResponse.Success)
        {
            string ErrorCode = (serviceResponse.ErrorException?.ErrorCode ?? "") == "" ? _loc["_FailedAction"] : _loc[$"_ERROR_{serviceResponse.ErrorException?.ErrorCode}"];
            _snackbar.Add($"{ErrorCode} {_loc["_PostCategory_Other_Langauge"]}", Severity.Error);
            return;
        }

        var parsedResponse = serviceResponse.ResponseData.CastModel<Dotnetable.Shared.DTO.Public.PublicActionResponse>();
        if (parsedResponse.SuccessAction)
        {
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_PostCategory_Other_Langauge"]}", Severity.Success);
        }

    }

    #endregion


}
