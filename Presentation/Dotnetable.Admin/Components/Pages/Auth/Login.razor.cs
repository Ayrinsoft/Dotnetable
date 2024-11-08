using Blazored.LocalStorage;
using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices.Authorization;
using Dotnetable.Service;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dotnetable.Admin.Components.Pages.Auth;

public partial class Login
{
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }
    [Inject] private AuthenticationStateProvider _authenticationStateProvider { get; set; }
    [Inject] private NavigationManager _navigationManager { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private ILocalStorageService _localStorage { get; set; }
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IConfiguration _appSettingsConfig { get; set; }
    [Inject] private IJSRuntime _jsRuntime { get; set; }
    [Inject] private AuthenticationService _auth { get; set; }
    [Inject] private IConfiguration _config { get; set; }

    private Dotnetable.Shared.DTO.Authentication.UserLoginRequest _loginRequest;

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
            var gRecaptchaCheck = await GeneralEvents.HttpClientReceive(HttpMethod.Get, $"https://google.com/recaptcha/api/siteverify?secret={recaptcahPrivate}&response={currentToken}", contentTypeRequest: Dotnetable.Shared.DTO.Public.RequestContentType.None);
            if (gRecaptchaCheck is null || !gRecaptchaCheck.IsSuccess)
            {
                _snackbar.Add($"{_loc[(gRecaptchaCheck.ErrorException?.ErrorCode is null ? "_ERROR_NULLDATA" : $"_ERROR_{gRecaptchaCheck.ErrorException.ErrorCode}")]} {_loc["_Login"]}", Severity.Error);
                return;
            }

            var recaptchaResponseDetail = gRecaptchaCheck.ResponseBody.ToString().JsonToObject<Dotnetable.Shared.DTO.Public.GoogleRecaptchaResponse>();
            if (!recaptchaResponseDetail.success || recaptchaResponseDetail.score <= 0.3)
            {
                _snackbar.Add($"{_loc["_ERROR_C17"]} {_loc["_Login"]}", Severity.Error);
                return;
            }
        }

        var parsedUserDetail = await _auth.LoginUser(_loginRequest, LocalSecret.TokenHashKey(_config["AdminPanelSettings:ClientHash"]));
        if (parsedUserDetail.ErrorException is not null)
        {
            _snackbar.Add($"{_loc["_ERROR_C1"]} {_loc["_Login"]}", Severity.Error);
            return;
        }

        await ((SharedServices.CustomAuthentication)_authenticationStateProvider).MarkUserAsAuthenticated(parsedUserDetail);
        _navigationManager.NavigateTo("/");
    }

}