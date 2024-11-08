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

public partial class Reset
{
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }
    [Inject] private AuthenticationStateProvider _authenticationStateProvider { get; set; }
    [Inject] private NavigationManager _navigationManager { get; set; }
    [Inject] private MemberService _member { get; set; }
    [Inject] private AuthenticationService _auth { get; set; }
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

        var recoveryDetail = await _member.ForgetPasswordGetCode(_fetchRecoveryCode);
        if (!recoveryDetail.SuccessAction)
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

        var recoveryDetail = await _member.ForgetPasswordSetCode(_setRecoveryCodeModel);
        if (!recoveryDetail.SuccessAction)
        {
            _snackbar.Add($"{_loc[(recoveryDetail.ErrorException?.ErrorCode is null ? "_ERROR_NULLDATA" : $"_ERROR_{recoveryDetail.ErrorException.ErrorCode}")]}", Severity.Error);
        }
        else
        {
            var userDetail = await _auth.LoginUser(new() { Username = _setRecoveryCodeModel.Username, Password = _setRecoveryCodeModel.Password }, LocalSecret.TokenHashKey(_config["AdminPanelSettings:ClientHash"]));
            if (userDetail.ErrorException is not null)
            {
                _snackbar.Add($"{_loc[(recoveryDetail.ErrorException?.ErrorCode is null ? "_ERROR_NULLDATA" : $"_ERROR_{recoveryDetail.ErrorException.ErrorCode}")]}", Severity.Error);
            }
            else
            {
                await ((SharedServices.CustomAuthentication)_authenticationStateProvider).MarkUserAsAuthenticated(userDetail);
                _navigationManager.NavigateTo("/");
            }
        }

    }

}
