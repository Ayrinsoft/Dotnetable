using Blazored.LocalStorage;
using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;
using System.Globalization;

namespace Dotnetable.Admin.Components.Pages.Auth;

public partial class Reset
{
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }
    [Inject] private AuthenticationStateProvider _authenticationStateProvider { get; set; }
    [Inject] private NavigationManager _navigationManager { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private ILocalStorageService _localStorage { get; set; }
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IJSRuntime _jsRuntime { get; set; }
    [Inject] private IConfiguration _config { get; set; }

    private Dotnetable.Shared.DTO.Member.MemberForgetPasswordRequest _fetchRecoveryCode;
    private Dotnetable.Shared.DTO.Member.MemberForgetPasswordSetRequest _setRecoveryCodeModel;
    private bool _persianLanguage = false;
    private bool _sendCode = false;
    private string _confirmPassword = "";

    protected override void OnInitialized()
    {
        _fetchRecoveryCode = new();
        _setRecoveryCodeModel = new();
    }

    private async Task GetRecoveryCode()
    {
        string recaptcahPrivate = _config["AdminPanelSettings:RecaptchaPrivateKey"];
        string recaptcahPublic = _config["AdminPanelSettings:RecaptchaPublicKey"];
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

        var recoveryDetail = await _httpService.CallServiceObjAsync(HttpMethod.Post, false, "Member/ForgetPasswordGetCode", _fetchRecoveryCode.ToJsonString());
        if (!recoveryDetail.Success)
        {
            _snackbar.Add($"{_loc[(recoveryDetail.ErrorException?.ErrorCode is null ? "_ERROR_NULLDATA" : $"_ERROR_{recoveryDetail.ErrorException.ErrorCode}")]} {(recoveryDetail?.ErrorException?.Message ?? "")}", Severity.Error);
        }
        else
        {
            _sendCode = true;
            _setRecoveryCodeModel = new() { Username = _fetchRecoveryCode.Username };
        }
    }

    private async Task SetRecoveryCode()
    {
        if (_confirmPassword != _setRecoveryCodeModel.Password) return;

        string recaptcahPrivate = _config["AdminPanelSettings:RecaptchaPrivateKey"];
        string recaptcahPublic = _config["AdminPanelSettings:RecaptchaPublicKey"];
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

        var recoveryDetail = await _httpService.CallServiceObjAsync(HttpMethod.Post, false, "Member/ForgetPasswordSetCode", _setRecoveryCodeModel.ToJsonString());
        if (!recoveryDetail.Success)
        {
            _snackbar.Add($"{_loc[(recoveryDetail.ErrorException?.ErrorCode is null ? "_ERROR_NULLDATA" : $"_ERROR_{recoveryDetail.ErrorException.ErrorCode}")]}", Severity.Error);
        }
        else
        {
            var userDetail = await _httpService.CallServiceObjAsync(HttpMethod.Post, false, "Authentication/Login", new Dotnetable.Shared.DTO.Authentication.UserLoginRequest() { Username = _setRecoveryCodeModel.Username, Password = _setRecoveryCodeModel.Password }.ToJsonString());
            if (!userDetail.Success)
            {
                _snackbar.Add($"{_loc[(recoveryDetail.ErrorException?.ErrorCode is null ? "_ERROR_NULLDATA" : $"_ERROR_{recoveryDetail.ErrorException.ErrorCode}")]}", Severity.Error);
            }
            else
            {
                var parsedUserDetail = userDetail.ResponseData.CastModel<Dotnetable.Shared.DTO.Authentication.UserLoginResponse>();
                await ((SharedServices.CustomAuthentication)_authenticationStateProvider).MarkUserAsAuthenticated(parsedUserDetail);
                _navigationManager.NavigateTo("/");
            }
        }

    }

}
