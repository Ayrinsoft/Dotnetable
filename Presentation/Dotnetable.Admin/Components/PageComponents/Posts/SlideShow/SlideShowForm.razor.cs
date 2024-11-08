using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.File;
using Dotnetable.Shared.DTO.Post;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Posts.SlideShow;

public partial class SlideShowForm
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private PostService _post { get; set; }
    [Inject] private FileService _fileService { get; set; }
    [Inject] private Tools _tools { get; set; }
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IHttpContextAccessor _httpContextAccessor { get; set; }

    [Parameter] public SlideShowInsertRequest FormModel { get; set; }
    [Parameter] public EventCallback<SlideShowInsertRequest> OnSubmitObject { get; set; }

    private byte[] _currentFileStream { get; set; }
    private string _currentFileName = "";
    private Dictionary<string, string> _slideShowSettings = new();
    private string _settingKey = "";
    private string _settingValue = "";
    private HttpContext _context = null;
    int memberID = -1;

    protected override async Task OnInitializedAsync()
    {
        memberID = await _tools.GetRequesterMemberID();
        _context = _httpContextAccessor.HttpContext;
        FormModel ??= new() { PageCode = "MAIN", Priority = 1 };
        if (!string.IsNullOrEmpty(FormModel.FileCode)) _currentFileName = FormModel.FileCode;
        if (!string.IsNullOrEmpty(FormModel.SettingsArray) && FormModel.SettingsArray != "{}") _slideShowSettings = FormModel.SettingsArray.JsonToObject<Dictionary<string, string>>();
    }

    private async Task OnSubmitForm()
    {
        if (_slideShowSettings is not null && _slideShowSettings.Count > 0)
            FormModel.SettingsArray = _slideShowSettings.ToJsonString();
        else
            FormModel.SettingsArray = "[]";

        await OnSubmitObject.InvokeAsync(FormModel);
    }

    private async Task DoDeleteFile(int? slideShowID)
    {
        FormModel.FileCode = string.Empty;
        if (slideShowID.HasValue && slideShowID.Value > 0)
            _ = await _post.RemoveSlideShowFile(new() { SlideShowID = slideShowID.Value });
    }

    private void AddNewSetting()
    {
        if (!string.IsNullOrEmpty(_settingKey) && _settingKey != "" && !string.IsNullOrEmpty(_settingValue) && _settingValue != "")
        {
            _slideShowSettings.Add(_settingKey, _settingValue);
            _settingKey = _settingValue = "";
        }
    }

    private void RemoveSetting(string settingKey) => _slideShowSettings.Remove(settingKey);


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

    private async Task DoUploadFile()
    {
        FileInsertRequest requestBody = new() { FileName = _currentFileName.FileNameCorrection(), FileStream = _currentFileStream, FileCode = Guid.NewGuid().ToString(), UploaderID = memberID };
        var uploadFile = await _fileService.Insert(requestBody);
        if (uploadFile.SuccessAction)
        {
            FormModel.FileCode = requestBody.FileCode;
            StateHasChanged();
        }
        else
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_FileUpload"]}", Severity.Error);
        }
    }

    private void ClearUploadFile()
    {
        _currentFileName = "";
        _currentFileStream = null;
    }



}