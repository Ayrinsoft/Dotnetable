using Blazored.LocalStorage;
using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using MudBlazor;
using MudBlazor.Utilities;

namespace Dotnetable.Admin.Components.Member.Profile;

public partial class ManageAvatar
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private ILocalStorageService _localStorage { get; set; }
    [Inject] private IHttpContextAccessor _httpContextAccessor { get; set; }

    [Parameter] public Dotnetable.Shared.DTO.Member.MemberDetailResponse MemberDetail { get; set; }

    private string _classname => new CssBuilder().AddClass(Class).Build();
    private byte[] _currentFileStream = null;
    private string _selectedFileName = "";
    private static readonly string _defaultDragClass = "relative rounded-lg border-2 border-dashed pa-4 mt-4 mud-width-full mud-height-full z-10";
    private string _dragClass = _defaultDragClass;
    private HttpContext _context = null;

    protected override void OnInitialized()
    {
        _context = _httpContextAccessor.HttpContext;
    }

    private async Task DoUploadFile()
    {
        if (_selectedFileName == "") return;
        var requestBody = new Dotnetable.Shared.DTO.Member.MemberAvatarInsertRequest() { FileName = $"{MemberDetail.Username}-Avatar.png", FileStream = _currentFileStream, SetAsDefault = true, CurrentMemberID = MemberDetail.MemberID };
        var uploadAvatar = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, $"Member/AvatarInsert", requestBody.ToJsonString());
        if (uploadAvatar.Success)
        {
            var parsedUploadAvatar = uploadAvatar.ResponseData.CastModel<Dotnetable.Shared.DTO.Public.PublicActionResponse>();
            MemberDetail.AvatarID = new Guid(parsedUploadAvatar.ObjectID);
            _snackbar.Add(_loc["_SuccessUploadFile"], Severity.Success);

            var jsonIdentity = await _localStorage.GetItemAsync<Dotnetable.Shared.DTO.Authentication.UserLoginResponse>("MemberAuthorized");
            jsonIdentity.AvatarID = MemberDetail.AvatarID;
            await _localStorage.SetItemAsync("MemberAuthorized", jsonIdentity);
            base.StateHasChanged();
        }
    }

    private async Task UploadFiles(InputFileChangeEventArgs e)
    {
        ClearDragClass();
        var uploadedFiles = e.GetMultipleFiles();
        if (uploadedFiles is null || uploadedFiles.Count == 0) return;
        var currentFile = uploadedFiles[0];
        if (currentFile is null) return;
        if (!string.IsNullOrEmpty(currentFile.Name)) _snackbar.Add($"{_loc["_SuccessFetchFile"]} - File name: {currentFile.Name}", Severity.Info);
        _selectedFileName = currentFile.Name;

        using MemoryStream ms = new();
        await currentFile.OpenReadStream().CopyToAsync(ms);
        _currentFileStream = ms.ToArray();
    }

    private void ClearUploadFile()
    {
        _selectedFileName = "";
        _currentFileStream = null;
        ClearDragClass();
    }

    private void SetDragClass() => _dragClass = $"{_defaultDragClass} mud-border-primary";
    private void ClearDragClass() => _dragClass = _defaultDragClass;


}
