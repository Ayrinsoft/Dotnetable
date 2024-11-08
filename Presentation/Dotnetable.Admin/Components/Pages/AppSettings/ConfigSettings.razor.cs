using Dotnetable.Admin.Components.Shared.Dialogs;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Website;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using MudBlazor;
using System.Globalization;

namespace Dotnetable.Admin.Components.Pages.AppSettings;

public partial class ConfigSettings
{
    [Inject] private NavigationManager _navigationManager { get; set; }
    [Inject] private IConfiguration _appSettingsConfig { get; set; }
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private WebsiteService _website { get; set; }
    [Inject] private IDialogService _dialogService { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }


    private string _serverAddress = "";
    private string _serverPort = "";
    private string _databaseName = "";
    private string _dbUsername = "";
    private string _dbPassword = "";
    private string _insertedLanguageCode = "";
    private string _confirmPassword = "";
    private bool _insertMemberData = false;
    private AdminPanelAppSettingsResponse _appSettings { get; set; }
    private AdminPanelFirstDataRequest _firstData = new() { AvailableLanguages = new() };

    protected override void OnInitialized()
    {
        _appSettings = new() { AdminPanelSettings = new(), ConnectionStrings = new(), DBSettings = new() };

        CultureInfo cultureInfo = new("en-US");
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        if (_appSettingsConfig["InsertDataMode"] == "COMPLETE" && !string.IsNullOrEmpty(_appSettingsConfig["AdminPanelSettings:DefaultLanguageCode"]) && _appSettingsConfig["AdminPanelSettings:DefaultLanguageCode"] != "")
            _navigationManager.NavigateTo("/");

        if (_appSettingsConfig["InsertDataMode"] == "INSERT_DATA")
            _insertMemberData = true;
    }


    //in debug mode it change root appsettigs file
    private async Task SaveSettings()
    {
        if (string.IsNullOrEmpty(_serverAddress) || _serverAddress == "" || string.IsNullOrEmpty(_serverPort) || _serverPort == "" || string.IsNullOrEmpty(_databaseName) || _databaseName == "" || string.IsNullOrEmpty(_dbUsername) || _dbUsername == "" || string.IsNullOrEmpty(_dbPassword) || _dbPassword == "" || string.IsNullOrEmpty(_appSettings.DBSettings.DBType) || _appSettings.DBSettings.DBType == "")
        {
            _snackbar.Add("Fill all items", Severity.Error);
            return;
        }

        _appSettings.AdminPanelSettings.DefaultLanguageCode = _appSettings.AdminPanelSettings.DefaultLanguageCode.ToUpper();

        _ = Enum.TryParse(_appSettings.DBSettings.DBType, out Dotnetable.Shared.DTO.Public.DatabaseType _dbTypeEnum);
        _appSettings.ConnectionStrings.DotnetableConnection = _dbTypeEnum switch
        {
            Dotnetable.Shared.DTO.Public.DatabaseType.POSTGRESQL => $"Server={_serverAddress};Port={_serverPort};Database={_databaseName};User Id={_dbUsername};Password={_dbPassword};",
            Dotnetable.Shared.DTO.Public.DatabaseType.MARIADB or Dotnetable.Shared.DTO.Public.DatabaseType.MYSQL => $"server={_serverAddress};port={_serverPort};database={_databaseName};uid={_dbUsername};pwd={_dbPassword};",
            _ => $"Server={_serverAddress}{(_serverPort == "1433" ? "" : $", {_serverPort}")};Database={_databaseName};User Id={_dbUsername};Password={_dbPassword};TrustServerCertificate=True;"
        };

        _appSettingsConfig.GetSection("AppSettings").Bind(_appSettings);

        File.WriteAllText("appsettings.json", _appSettings.ToJsonString());

        var dbCreateDetail = await _website.ImplementDB(_appSettings.AdminPanelSettings.DefaultLanguageCode);
        if (!dbCreateDetail.SuccessAction)
        {
            _snackbar.Add(dbCreateDetail?.ErrorException?.Message, Severity.Error);
            return;
        }

        _snackbar.Add("Items saved successfully", Severity.Success);
        _insertMemberData = true;
        _firstData.AvailableLanguages.Add(_appSettings.AdminPanelSettings.DefaultLanguageCode);
    }

    private void AppendLanguageToList()
    {
        if (string.IsNullOrEmpty(_insertedLanguageCode) || _insertedLanguageCode == "") return;
        if (_firstData.AvailableLanguages.Any(i => i.Contains(_insertedLanguageCode, StringComparison.OrdinalIgnoreCase)) || _insertedLanguageCode.Length != 2)
        {
            _insertedLanguageCode = "";
            return;
        }
        _firstData.AvailableLanguages.Add(_insertedLanguageCode.ToUpper());
        _insertedLanguageCode = "";
    }

    private void CheckForEnter(KeyboardEventArgs e)
    {
        if (e.Code == "Enter")
        {
            AppendLanguageToList();
        }
    }

    private async Task Removelanguage(string languageCode)
    {
        if ((await _dialogService.Show<ConfirmDialog>(_loc["_AreYouSure"], new DialogOptions { CloseOnEscapeKey = true, CloseButton = true, MaxWidth = MaxWidth.Small, Position = DialogPosition.Center }).Result).Canceled)
            return;

        _firstData.AvailableLanguages.Remove(languageCode);
    }

    private async Task SaveUserData()
    {
        if (_confirmPassword != _firstData.Password)
            return;

        var dbCreateDetail = await _website.InsertFirstData(_firstData);
        if (!dbCreateDetail.SuccessAction)
        {
            _snackbar.Add(dbCreateDetail?.ErrorException?.Message, Severity.Error);
            return;
        }

        _snackbar.Add("Items saved successfully", Severity.Success);
        _navigationManager.NavigateTo("/");
    }

}