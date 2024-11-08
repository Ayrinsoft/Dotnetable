using Blazored.LocalStorage;
using Dotnetable.Admin.Models;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Authentication;
using Dotnetable.Shared.DTO.Post;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dotnetable.Admin.Components.Pages.Posts.Post;

public partial class QRCodeManage
{
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private PostService _post { get; set; }
    [Inject] private ILocalStorageService _localStorage { get; set; }
    [Inject] private IJSRuntime _jsRuntime { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }


    private bool _showLanguageSelected = true;
    private string _selectedLanguageCode = "";
    private UserLoginResponse.TokenItems _fetchCurrentToken = null;
    private string _ckContainerID = "";
    private QRCodeUpdateRequest _qrCodeModel { get; set; } = new() { QRCodeDetail = new(), PublicPostDetail = new() };
    private PostDetailPublicResponse _publicPostDetail { get; set; } = new();
    private Dictionary<string, string> _otherParts = new();


    protected async override Task OnInitializedAsync()
    {
        if (await _localStorage.ContainKeyAsync("JToken"))
            _fetchCurrentToken = await _localStorage.GetItemAsync<UserLoginResponse.TokenItems>("JToken");

        _ckContainerID = $"ck{Guid.NewGuid().ToString().Replace("-", "")}";

        var fetchServiceData = await _post.PublicPostDetail(new() { PostCode = "QRCode" });
        if (fetchServiceData.ErrorException is null)
        {
            _publicPostDetail = fetchServiceData;
        }
        else
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc[$"_ERROR_{(fetchServiceData.ErrorException?.ErrorCode ?? "SX")}"]}", Severity.Error);
        }
    }

    private async Task checkForSend(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && _selectedLanguageCode.Length == 2)
            await ChangeLanguageCode();
    }

    private async Task ChangeLanguageCode()
    {
        await _jsRuntime.InvokeVoidAsync("Plugin.CKEditorLunch", new object[] { _ckContainerID, _fetchCurrentToken.Token ?? "" });
        _selectedLanguageCode = _selectedLanguageCode.ToUpper();
        if (_publicPostDetail is null || _publicPostDetail.PostsDetail is null)
        {
            _qrCodeModel.PublicPostDetail = new();
            _qrCodeModel.QRCodeDetail = new();
            return;
        }

        var fetchLanguage = (from i in _publicPostDetail.PostsDetail where i.LanguageCode == _selectedLanguageCode select i).FirstOrDefault();
        if (fetchLanguage is not null)
        {
            _qrCodeModel.PublicPostDetail = fetchLanguage.CastModel<PostPublicPageDetailUpdateRequest>();
            _qrCodeModel.QRCodeDetail = fetchLanguage.Body.JsonToObject<StaticPageDetailQRCodeResponse>();
            _otherParts = _qrCodeModel.QRCodeDetail.OtherHtmlPart;
        }
        else
        {
            _qrCodeModel.PublicPostDetail = new();
            _qrCodeModel.QRCodeDetail = new();
            _otherParts = new();
        }

        _showLanguageSelected = false;
        await _jsRuntime.InvokeVoidAsync("Plugin.CKEditorSetData", _qrCodeModel?.QRCodeDetail?.HTMLBody ?? "");
    }


    private async Task UpdateModel()
    {
        _qrCodeModel.QRCodeDetail.HTMLBody = await _jsRuntime.InvokeAsync<string>("Plugin.CKEditorGetData");
        _qrCodeModel.QRCodeDetail.OtherHtmlPart = _otherParts;

        var fetchServiceData = await _post.QRCodeUpdate(_qrCodeModel);
        if (fetchServiceData.ErrorException is null)
        {
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_QRCodeManage"]}", Severity.Success);
        }
        else
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc[$"_ERROR_{(fetchServiceData.ErrorException?.ErrorCode ?? "SX")}"]}", Severity.Error);
        }
    }

    private string _partKey = "";
    private string _partValue = "";
    private void AddNewPart()
    {
        _otherParts ??= [];
        if (_partKey == "" || _partValue == "") return;
        if (_otherParts.Any(i => i.Key == _partKey))
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_ERROR_D2"]}", Severity.Error);
            return;
        }
        _otherParts.Add(_partKey, _partValue);
        _partKey = _partValue = "";
    }

    private void RemovePart(string partKey) => _otherParts.Remove(partKey);
}
