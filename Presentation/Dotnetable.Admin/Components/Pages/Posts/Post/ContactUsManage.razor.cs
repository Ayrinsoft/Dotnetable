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
using OpenLayers.Blazor;

namespace Dotnetable.Admin.Components.Pages.Posts.Post;

public partial class ContactUsManage
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
    private ContactUsUpdateRequest _contactModel = new() { ContactUsDetail = new(), PublicPostDetail = new() };
    private PostDetailPublicResponse _publicPostDetail = new();
    private Dictionary<string, string> _addresses = new();
    private Dictionary<string, string> _emails = new();
    private Dictionary<string, string> _phoneNumbers = new();
    private Dictionary<string, string> _faxNumbers = new();
    private List<StaticPageDetailContactUsResponse.WorkingHours> _workingHours = new();
    private OpenStreetMap _map = null!;

    protected async override Task OnInitializedAsync()
    {
        if (await _localStorage.ContainKeyAsync("JToken"))
            _fetchCurrentToken = await _localStorage.GetItemAsync<UserLoginResponse.TokenItems>("JToken");

        _ckContainerID = $"ck{Guid.NewGuid().ToString().Replace("-", "")}";
        var fetchServiceData = await _post.PublicPostDetail(new() { PostCode = "ContactUs" });
        if (fetchServiceData.ErrorException is null)
        {
            _publicPostDetail = fetchServiceData;
        }
        else
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc[$"_ERROR_{(fetchServiceData.ErrorException?.ErrorCode ?? "SX")}"]}", Severity.Error);
        }
    }

    private async Task CheckForSend(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && _selectedLanguageCode.Length == 2) await ChangeLanguageCode();
    }

    private async Task ChangeLanguageCode()
    {
        await _jsRuntime.InvokeVoidAsync("Plugin.CKEditorLunch", new object[] { _ckContainerID, _fetchCurrentToken.Token ?? "" });
        _map.Zoom = 11;
        _map.Center = new Coordinate(35.95491763510364, 52.10960933412857);

        _selectedLanguageCode = _selectedLanguageCode.ToUpper();
        if (_publicPostDetail is null || _publicPostDetail.PostsDetail is null)
        {
            _contactModel.PublicPostDetail = new();
            _contactModel.ContactUsDetail = new();
            return;
        }

        var fetchLanguage = (from i in _publicPostDetail.PostsDetail where i.LanguageCode == _selectedLanguageCode select i).FirstOrDefault();
        if (fetchLanguage is not null)
        {
            _contactModel.PublicPostDetail = fetchLanguage.CastModel<PostPublicPageDetailUpdateRequest>();
            _contactModel.ContactUsDetail = fetchLanguage.Body.JsonToObject<StaticPageDetailContactUsResponse>();

            _addresses = _contactModel.ContactUsDetail.Address;
            _emails = _contactModel.ContactUsDetail.EmailAddress;
            _phoneNumbers = _contactModel.ContactUsDetail.PhoneNumber;
            _faxNumbers = _contactModel.ContactUsDetail.Faxnumber;
            _workingHours = _contactModel.ContactUsDetail.WorkHours;


            if (!string.IsNullOrEmpty(_contactModel.ContactUsDetail.MapLocationLatitude) && _contactModel.ContactUsDetail.MapLocationLatitude != "" && !string.IsNullOrEmpty(_contactModel.ContactUsDetail.MapLocationLongitude) && _contactModel.ContactUsDetail.MapLocationLongitude != "" && double.TryParse(_contactModel.ContactUsDetail.MapLocationLongitude, out double _locationLongitude) && double.TryParse(_contactModel.ContactUsDetail.MapLocationLatitude, out double _locationLatitude))
            {
                _map.MarkersList.Add(new Marker() { Type = MarkerType.MarkerPin, Popup = true, PinColor = PinColor.Red, Coordinate = new(_locationLatitude, _locationLongitude), Text = "Current location" });
                _map.Center = new Coordinate(_locationLatitude, _locationLongitude);
            }
        }
        else
        {
            _contactModel.PublicPostDetail = new() { LanguageCode = _selectedLanguageCode };
            _contactModel.ContactUsDetail = new();
            _emails = new();
            _phoneNumbers = new();
            _faxNumbers = new();
            _addresses = new();
            _workingHours = new();
        }

        _showLanguageSelected = false;
        await _jsRuntime.InvokeVoidAsync("Plugin.CKEditorSetData", _contactModel?.ContactUsDetail?.HTMLBody ?? "");
    }

    private async Task UpdateModel()
    {
        _contactModel.ContactUsDetail.HTMLBody = await _jsRuntime.InvokeAsync<string>("Plugin.CKEditorGetData");
        _contactModel.ContactUsDetail.Address = _addresses;
        _contactModel.ContactUsDetail.EmailAddress = _emails;
        _contactModel.ContactUsDetail.PhoneNumber = _phoneNumbers;
        _contactModel.ContactUsDetail.Faxnumber = _faxNumbers;
        _contactModel.ContactUsDetail.WorkHours = _workingHours;
        _contactModel.ContactUsDetail.MapLocationLatitude = _locationLatitudeNew;
        _contactModel.ContactUsDetail.MapLocationLongitude = _locationLongitudeNew;

        var fetchServiceData = await _post.ContactusUpdate(_contactModel);
        if (fetchServiceData.SuccessAction)
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_ContactUsManage"]}", Severity.Success);
        else
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc[$"_ERROR_{(fetchServiceData.ErrorException?.ErrorCode ?? "SX")}"]}", Severity.Error);
    }

    #region Address
    private string _addressKey = "";
    private string _addressValue = "";
    private void AddNewAddress()
    {
        _addresses ??= [];
        if (_addressKey == "" || _addressValue == "") return;
        if (_addresses.Any(i => i.Key == _addressKey))
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_ERROR_D2"]}", Severity.Error);
            return;
        }
        _addresses.Add(_addressKey, _addressValue);
        _addressKey = _addressValue = "";
    }

    private void RemoveAddress(string addressKey) => _addresses.Remove(addressKey);
    #endregion

    #region Emails
    private string _emailKey = "";
    private string _emailValue = "";
    private void AddNewEmail()
    {
        _emails ??= [];
        if (_emailValue == "" || _emailKey == "") return;
        if (_emails.Any(i => i.Value == _emailValue || i.Key == _emailKey))
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_ERROR_D2"]}", Severity.Error);
            return;
        }
        _emails.Add(_emailKey, _emailValue);
        _emailValue = _emailKey = "";
    }
    private void RemoveEmail(string emailkey) => _emails.Remove(emailkey);

    #endregion

    #region Phone
    private string _phoneKey = "";
    private string _phoneValue = "";
    private void AddNewPhone()
    {
        _phoneNumbers ??= [];
        if (_phoneValue == "" || _phoneKey == "") return;
        if (_phoneNumbers.Any(i => i.Value == _phoneValue || i.Key == _phoneKey))
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_ERROR_D2"]}", Severity.Error);
            return;
        }
        _phoneNumbers.Add(_phoneKey, _phoneValue);
        _phoneValue = _phoneKey = "";
    }
    private void RemovePhone(string phoneKey) => _phoneNumbers.Remove(phoneKey);

    #endregion

    #region Fax
    private string _faxKey = "";
    private string _faxValue = "";
    private void AddNewFax()
    {
        _faxNumbers ??= [];
        if (_faxValue == "" || _faxKey == "") return;
        if (_faxNumbers.Any(i => i.Value == _faxValue || i.Key == _faxKey))
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_ERROR_D2"]}", Severity.Error);
            return;
        }
        _faxNumbers.Add(_faxKey, _faxValue);
        _faxValue = _faxKey = "";
    }

    private void RemoveFax(string faxValue) => _faxNumbers.Remove(faxValue);

    #endregion

    #region WorkingHours
    private string _workingFrom = "";
    private string _workingTo = "";
    private string _workingWeekDays = "";
    private void AddNewWorkingHours()
    {
        _workingHours ??= [];
        if (_workingFrom == "" || _workingTo == "" || _workingWeekDays == "") return;
        if (_workingHours.Any(i => i.ToHour == _workingTo || i.FromHour == _workingFrom || i.WeekDays == _workingWeekDays))
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_ERROR_D2"]}", Severity.Error);
            return;
        }
        _workingHours.Add(new() { WeekDays = _workingWeekDays, FromHour = _workingFrom, ToHour = _workingTo });
        _workingFrom = _workingTo = _workingWeekDays = "";
    }

    private void RemoveWorkingHours(StaticPageDetailContactUsResponse.WorkingHours workingHoursItem) => _workingHours.Remove(workingHoursItem);
    #endregion


    #region Map
    private string _locationLatitudeNew = string.Empty;
    private string _locationLongitudeNew = string.Empty;
    private void OnMapClick(Coordinate coordinate)
    {
        _locationLatitudeNew = coordinate.Latitude.ToString();
        _locationLongitudeNew = coordinate.Longitude.ToString();
        _map.MarkersList.Clear();

        //Console.WriteLine(coordinate.ToJsonString()); //{"Latitude":41.03607760434852,"Y":41.03607760434852,"Longitude":28.857001228660334,"X":28.857001228660334,"Coordinates":[28.857001228660334,41.03607760434852]}
        _map.MarkersList.Add(new Marker() { Type = MarkerType.MarkerPin, Popup = true, PinColor = PinColor.Blue, Coordinate = coordinate, Text = "Point of your location" });
    }

    #endregion


}
