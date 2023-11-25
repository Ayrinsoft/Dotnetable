using Blazored.LocalStorage;
using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.DTO.Authentication;
using Dotnetable.Shared.DTO.Post;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dotnetable.Admin.Components.Pages.Posts.Post;

public partial class AboutUsManage
{
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }
    [Inject] private ILocalStorageService _localStorage { get; set; }
    [Inject] private IJSRuntime _jsRuntime { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    private bool _showLanguageSelected = true;
    private string _selectedLanguageCode = "";
    private UserLoginResponse.TokenItems _fetchCurrentToken = null;
    string _ckContainerID = "";
    private AboutUsUpdateRequest _aboutModel = new() { AboutusDetail = new(), PublicPostDetail = new() };
    private PostDetailPublicResponse _publicPostDetail = new();
    private Dictionary<string, string> _otherParts = new();
    private Dictionary<string, string> _relatedCompanies = new();

    protected async override Task OnInitializedAsync()
    {
        if (await _localStorage.ContainKeyAsync("JToken"))
            _fetchCurrentToken = await _localStorage.GetItemAsync<UserLoginResponse.TokenItems>("JToken");

        _ckContainerID = $"ck{Guid.NewGuid().ToString().Replace("-", "")}";
        var fetchServiceData = await _httpService.CallServiceObjAsync(HttpMethod.Post, false, "Post/PublicPostDetail", new PostDetailPublicRequest { PostCode = "Aboutus" }.ToJsonString());
        if (fetchServiceData.Success)
        {
            _publicPostDetail = fetchServiceData.ResponseData.CastModel<PostDetailPublicResponse>();
        }
        else
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc[$"_ERROR_{(fetchServiceData.ErrorException?.ErrorCode ?? "SX")}"]}", Severity.Error);
        }
    }

    private async Task CheckForSend(KeyboardEventArgs e)
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
            _aboutModel.PublicPostDetail = new();
            _aboutModel.AboutusDetail = new();
            return;
        }

        var fetchLanguage = (from i in _publicPostDetail.PostsDetail where i.LanguageCode == _selectedLanguageCode select i).FirstOrDefault();
        if (fetchLanguage is not null)
        {
            _aboutModel.PublicPostDetail = fetchLanguage.CastModel<PostPublicPageDetailUpdateRequest>();
            _aboutModel.AboutusDetail = fetchLanguage.Body.JsonToObject<StaticPageDetailAboutUsResponse>();
            _otherParts = _aboutModel.AboutusDetail.OtherHtmlPart;
            _relatedCompanies = _aboutModel.AboutusDetail.RelatedCompanies;
        }
        else
        {
            _aboutModel.PublicPostDetail = new();
            _aboutModel.AboutusDetail = new();
            _otherParts = new();
            _relatedCompanies = new();
        }

        _showLanguageSelected = false;
        await _jsRuntime.InvokeVoidAsync("Plugin.CKEditorSetData", _aboutModel?.AboutusDetail?.HTMLBody ?? "");
    }

    private async Task UpdateModel()
    {
        _aboutModel.AboutusDetail.HTMLBody = await _jsRuntime.InvokeAsync<string>("Plugin.CKEditorGetData");
        _aboutModel.AboutusDetail.OtherHtmlPart = _otherParts;
        _aboutModel.AboutusDetail.RelatedCompanies = _relatedCompanies;

        var fetchServiceData = await _httpService.CallServiceObjAsync(System.Net.Http.HttpMethod.Post, true, "Post/AboutusUpdate", _aboutModel.ToJsonString());
        if (fetchServiceData.Success)
        {
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_AboutUsManage"]}", Severity.Success);
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


    private string _companyKey = "";
    private string _companyValue = "";
    private void AddNewCompany()
    {
        if (_companyKey == "" || _companyValue == "") return;
        if (_otherParts.Any(i => i.Key == _companyKey))
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_ERROR_D2"]}", Severity.Error);
            return;
        }
        _relatedCompanies.Add(_companyKey, _companyValue);
        _companyKey = _companyValue = "";
    }
    private void RemoveCompany(string companyKey) => _relatedCompanies.Remove(companyKey);


}
