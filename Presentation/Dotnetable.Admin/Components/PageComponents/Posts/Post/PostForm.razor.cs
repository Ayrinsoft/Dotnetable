using Blazored.LocalStorage;
using Dotnetable.Admin.Components.Shared.Dialogs;
using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Authentication;
using Dotnetable.Shared.DTO.File;
using Dotnetable.Shared.DTO.Post;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Posts.Post;

public partial class PostForm
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private IDialogService _dialogService { get; set; }
    [Inject] private PostService _post { get; set; }
    [Inject] private FileService _fileService { get; set; }
    [Inject] private Tools _tools { get; set; }
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private ILocalStorageService _localStorage { get; set; }
    [Inject] private IJSRuntime _jsRuntime { get; set; }
    [Inject] private IHttpContextAccessor _httpContextAccessor { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    [Parameter] public PostUpdateRequest FormModel { get; set; }
    [Parameter] public string DefaultLanguageCode { get; set; }
    [Parameter] public EventCallback<PostUpdateRequest> OnSubmitObject { get; set; }
    [Parameter] public string FunctionName { get; set; }

    private byte[] _currentFileStream { get; set; }
    private string _currentFileName { get; set; }
    private HttpContext _context = null;
    private UserLoginResponse.TokenItems _fetchCurrentToken = null;
    private string _ckContainerID = $"ck{Guid.NewGuid().ToString().Replace("-", "")}";
    private string _postCatDefaultLanguage = "";
    int memberID = -1;

    protected async override Task OnInitializedAsync()
    {
        _postCatDefaultLanguage = themeManager.LanguageCode;
        _context = _httpContextAccessor.HttpContext;
        if (string.IsNullOrEmpty(DefaultLanguageCode) || DefaultLanguageCode == "") DefaultLanguageCode = themeManager.LanguageCode;
        FormModel ??= new() { LanguageCode = DefaultLanguageCode };
        if (FunctionName is null || FunctionName == "" || FunctionName == "Insert")
        {
            FunctionName = "Insert";
            FormModel = new PostUpdateRequest() { Body = "<p>.</p>", PostID = 1, LanguageCode = DefaultLanguageCode };
            await _localStorage.RemoveItemAsync("TMPFiles");
        }
        else
        {
            await _localStorage.RemoveItemAsync("TMPFiles");
            string fileList = "";
            if (FormModel.FileCodes is not null && FormModel.FileCodes.Count > 0)
            {
                foreach (var j in FormModel.FileCodes)
                {
                    if (fileList != "") fileList += ",";
                    fileList += j;
                }
            }
            await _localStorage.SetItemAsStringAsync("TMPFiles", fileList);
        }

        memberID = await _tools.GetRequesterMemberID();
    }

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _jsRuntime.InvokeVoidAsync("Plugin.SetCookie", new object[] { "TMPFiles", "", -1 });

            if (await _localStorage.ContainKeyAsync("JToken"))
            {
                _fetchCurrentToken = await _localStorage.GetItemAsync<UserLoginResponse.TokenItems>("JToken");
                await _jsRuntime.InvokeVoidAsync("Plugin.CKEditorLunch", new object[] { _ckContainerID, _fetchCurrentToken.Token ?? "" });
            }
        }
    }

    private async Task OnSubmitForm()
    {
        FormModel.Body = await _jsRuntime.InvokeAsync<string>("Plugin.CKEditorGetData");
        if (FunctionName == "UpdateLanguage") FormModel.Active = true;
        await OnSubmitObject.InvokeAsync(FormModel);
        StateHasChanged();
    }


    private async Task DeleteUploadedFile(string fileCode)
    {
        if ((await _dialogService.Show<ConfirmDialog>(_loc["_AreYouSure"]).Result).Canceled)
            return;

        var removeFile = await _post.RemovePostFile(new() { FileCode = fileCode, PostID = FormModel.PostID, CoverImage = false });
        if (removeFile.SuccessAction)
        {
            if (await _localStorage.ContainKeyAsync("TMPFiles"))
            {
                string fileListStorage = await _localStorage.GetItemAsStringAsync("TMPFiles");
                string fileListString = (fileListStorage ?? "").Replace(fileCode, "").Replace(",,", ",");
                await _localStorage.SetItemAsync("TMPFiles", fileListString);
            }

            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Delete"]}", Severity.Success);
            FormModel.FileCodes.Remove(fileCode);
        }
        else
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Delete"]}", Severity.Error);
        }
    }

    private void OnChangePostCategorySelected(int postCategoryID) => FormModel.PostCategoryID = postCategoryID;


    private async Task DoDeleteCoverFile()
    {
        if (await _localStorage.ContainKeyAsync("TMPFiles"))
        {
            string fileListStorage = await _localStorage.GetItemAsStringAsync("TMPFiles");
            if (!string.IsNullOrEmpty(fileListStorage) && fileListStorage != "")
            {
                string fileListString = fileListStorage.Replace(FormModel.MainImage?.ToString() ?? "", "").Replace(",,", ",");
                await _localStorage.SetItemAsStringAsync("TMPFiles", fileListString);
            }
        }

        if (FunctionName != "Insert")
            _ = await _post.RemovePostFile(new() { FileCode = FormModel.MainImage.ToString(), PostID = FormModel.PostID, CoverImage = true });

        FormModel.MainImage = null;
    }

    private async Task DoUploadFile()
    {
        if (FormModel.MainImage is not null && await _localStorage.ContainKeyAsync("TMPFiles"))
        {
            string fileListStorage = await _localStorage.GetItemAsStringAsync("TMPFiles");
            string fileListString = (fileListStorage ?? "").Replace(FormModel.MainImage?.ToString() ?? "", "").Replace(",,", ",");
            await _localStorage.SetItemAsStringAsync("TMPFiles", fileListString);
        }

        FileInsertRequest requestBody = new() { FileName = "CoverImage.jpg", FileStream = _currentFileStream, FileCode = Guid.NewGuid().ToString(), UploaderID = memberID };
        var uploadFile = await _fileService.Insert(requestBody);
        if (uploadFile.SuccessAction)
        {
            FormModel.MainImage = new(requestBody.FileCode);
            string fileListString = await _localStorage.GetItemAsStringAsync("TMPFiles") ?? "";
            if (fileListString != "") fileListString += ",";
            fileListString += FormModel.MainImage.ToString();
            await _localStorage.SetItemAsStringAsync("TMPFiles", fileListString);
            StateHasChanged();
        }
    }

    private void ClearUploadFile()
    {
        _currentFileName = "";
        _currentFileStream = null;
    }

    private async Task UploadFiles(InputFileChangeEventArgs e)
    {
        var uploadedFiles = e.GetMultipleFiles();
        if (uploadedFiles is null || uploadedFiles.Count == 0) return;
        var currentFile = uploadedFiles[0];
        if (currentFile is null) return;
        if (!string.IsNullOrEmpty(currentFile.Name)) _snackbar.Add($"{_loc["_SeccessFetchFile"]} - File name: {currentFile.Name}", Severity.Info);
        _currentFileName = currentFile.Name;

        using MemoryStream ms = new();
        await currentFile.OpenReadStream().CopyToAsync(ms);
        _currentFileStream = ms.ToArray();
    }

}
