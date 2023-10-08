using Blazored.LocalStorage;
using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;
using System.Globalization;
using static Org.BouncyCastle.Math.EC.ECCurve;
using System.Threading;
using Dotnetable.Admin.SharedServices;
using Dotnetable.Admin.Models;

namespace Dotnetable.Admin.Pages.Auth;

public partial class Login
{
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }
    [Inject] private AuthenticationStateProvider _authenticationStateProvider { get; set; }
    [Inject] private NavigationManager _navigationManager { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }
    [Inject] private IStringLocalizer<Shared.Resources.Resource> _loc { get; set; }
    [Inject] private ILocalStorageService _localStorage { get; set; }
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IConfiguration _appSettingsConfig { get; set; }
    [Inject] private IJSRuntime _jsRuntime { get; set; }

    private Shared.DTO.Authentication.UserLoginRequest _loginRequest;

    protected async override Task OnInitializedAsync()
    {
        string languageCode = _appSettingsConfig["AdminPanelSettings:DefaultLanguageCode"];
        string insertStatus = _appSettingsConfig["InsertDataMode"];

        if (string.IsNullOrEmpty(languageCode) || languageCode == "" || string.IsNullOrEmpty(insertStatus) || insertStatus == "" || insertStatus != "COMPLETE")
            _navigationManager.NavigateTo("/ConfigSettings");


        _loginRequest = new();

        string recaptcahPublic = _appSettingsConfig["AdminPanelSettings:RecaptchaPublicKey"];
        if (!string.IsNullOrEmpty(recaptcahPublic) && recaptcahPublic != "")
        {
            await _jsRuntime.InvokeVoidAsync("Plugin.loadRecaptcha", new object[] { recaptcahPublic });
        }

    }

    private async Task ValidateUser()
    {
        //for localhost test, append localhost to your google recaptcha sites
        string recaptcahPrivate = _appSettingsConfig["AdminPanelSettings:RecaptchaPrivateKey"];
        string recaptcahPublic = _appSettingsConfig["AdminPanelSettings:RecaptchaPublicKey"];
        if (!string.IsNullOrEmpty(recaptcahPrivate) && recaptcahPrivate != "" && !string.IsNullOrEmpty(recaptcahPublic) && recaptcahPublic != "")
        {
            string currentToken = await _jsRuntime.InvokeAsync<string>("Plugin.generateCaptchaToken", new object[] { recaptcahPublic, "login" });
            var gRecaptchaCheck = await GeneralEvents.HttpClientReceive(HttpMethod.Get, $"https://google.com/recaptcha/api/siteverify?secret={recaptcahPrivate}&response={currentToken}", contentTypeRequest: Shared.DTO.Public.RequestContentType.None);
            if (gRecaptchaCheck is null || !gRecaptchaCheck.IsSuccess)
            {
                _snackbar.Add($"{_loc[(gRecaptchaCheck.ErrorException?.ErrorCode is null ? "_ERROR_NULLDATA" : $"_ERROR_{gRecaptchaCheck.ErrorException.ErrorCode}")]} {_loc["_Login"]}", Severity.Error);
                return;
            }

            var recaptchaResponseDetail = gRecaptchaCheck.ResponseBody.ToString().JsonToObject<Shared.DTO.Public.GoogleRecaptchaResponse>();
            if (!recaptchaResponseDetail.success || recaptchaResponseDetail.score <= 0.3)
            {
                _snackbar.Add($"{_loc["_ERROR_C17"]} {_loc["_Login"]}", Severity.Error);
                return;
            }
        }

        var userDetail = await _httpService.CallServiceObjAsync(HttpMethod.Post, false, "Authentication/Login", _loginRequest.ToJsonString());
        if (!userDetail.Success)
        {
            _snackbar.Add($"{_loc[(userDetail.ErrorException?.ErrorCode is null ? "_ERROR_NULLDATA" : $"_ERROR_{userDetail.ErrorException.ErrorCode}")]} {_loc["_Login"]}", Severity.Error);
            return;
        }

        var parsedUserDetail = userDetail.ResponseData.CastModel<Shared.DTO.Authentication.UserLoginResponse>();
        await ((SharedServices.CustomAuthentication)_authenticationStateProvider).MarkUserAsAuthenticated(parsedUserDetail);
        _navigationManager.NavigateTo("/");
    }

}