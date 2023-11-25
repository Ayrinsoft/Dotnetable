using Dotnetable.Admin.Components.PageComponents.Member.Manage;
using Dotnetable.Admin.Components.PageComponents.Member.Profile;
using Dotnetable.Admin.Components.Shared.Dialogs;
using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.DTO.Member;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.Pages.Member;

public partial class MemberManage
{

    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }
    [Inject] private IDialogService _dialogService { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    private MemberListRequest _listRequest { get; set; }
    private MemberListFinalResponse _listResponse { get; set; }
    private GridViewHeaderParameters _gridHeaderParams { get; set; }


    #region Grid
    protected async override Task OnInitializedAsync()
    {
        _gridHeaderParams = new()
        {
            HeaderItems = new()
            {
                new() { ColumnLocalizeCode = "_Username", ColumnName = nameof(MemberListFinalResponse.MemberDetail.Username), HasSearch = true, SearchType = SearchColumnType.Text, HasSort = true },
                new() { ColumnLocalizeCode = "_Email", ColumnName = nameof(MemberListFinalResponse.MemberDetail.Email), HasSearch = true, SearchType = SearchColumnType.Text, HasSort = true },
                new() { ColumnLocalizeCode = "_Cellphone", ColumnName = nameof(MemberListFinalResponse.MemberDetail.CellphoneNumber), HasSearch = true, SearchType = SearchColumnType.Text, HasSort = true },
                new() { ColumnLocalizeCode = "_CountryCode" },
                new() { ColumnLocalizeCode = "_Gender", ColumnName = nameof(MemberListFinalResponse.MemberDetail.Gender), HasSearch = true, SearchType = SearchColumnType.CheckBox },
                new() { ColumnLocalizeCode = "_Givenname", ColumnName = nameof(MemberListFinalResponse.MemberDetail.Givenname), HasSearch = true, SearchType = SearchColumnType.Text, HasSort = true },
                new() { ColumnLocalizeCode = "_Surname", ColumnName = nameof(MemberListFinalResponse.MemberDetail.Surname), HasSearch = true, SearchType = SearchColumnType.Text, HasSort = true },
                new() { ColumnLocalizeCode = "_RegisterDate" },
                new() { ColumnLocalizeCode = "_Status" },
                new() { ColumnLocalizeCode = "_Active_DeActive" },
                new() { ColumnLocalizeCode = "_Management" }
            },
            Pagination = new() { MaxLength = _listResponse?.DatabaseRecords ?? 1, ShowFirstLast = true }
        };

        RefreshRequestInput();
        await FetchGrid();
    }

    private async Task OnSearchChanged(GridViewHeaderParameters changedColumns)
    {
        _gridHeaderParams = changedColumns;
        RefreshRequestInput();
        await FetchGrid();
    }

    private void RefreshRequestInput()
    {
        _listRequest = new()
        {
            SkipCount = ((_gridHeaderParams.Pagination.PageIndex - 1) * _gridHeaderParams.Pagination.PageSize),
            TakeCount = _gridHeaderParams.Pagination.PageSize,
            OrderbyParams = _gridHeaderParams.OrderbyParams,
            CellphoneNumber = _gridHeaderParams.HeaderItems.FirstOrDefault(i => i.ColumnName == nameof(MemberListFinalResponse.MemberDetail.CellphoneNumber)).SearchText,
            Email = _gridHeaderParams.HeaderItems.FirstOrDefault(i => i.ColumnName == nameof(MemberListFinalResponse.MemberDetail.Email)).SearchText,
            Givenname = _gridHeaderParams.HeaderItems.FirstOrDefault(i => i.ColumnName == nameof(MemberListFinalResponse.MemberDetail.Givenname)).SearchText,
            Surname = _gridHeaderParams.HeaderItems.FirstOrDefault(i => i.ColumnName == nameof(MemberListFinalResponse.MemberDetail.Surname)).SearchText,
            Username = _gridHeaderParams.HeaderItems.FirstOrDefault(i => i.ColumnName == nameof(MemberListFinalResponse.MemberDetail.Username)).SearchText,
        };
    }

    private async Task FetchGrid()
    {
        var fetchMembers = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Member/MemberList", _listRequest.ToJsonString());
        if (fetchMembers.Success)
        {
            _listResponse = fetchMembers.ResponseData.CastModel<MemberListFinalResponse>();
        }
        _gridHeaderParams.Pagination.MaxLength = _listResponse?.DatabaseRecords ?? 1;
        StateHasChanged();
    }
    #endregion

    #region ActiveDeActive

    private async Task InsertNewMember()
    {
        var checkInsert = await _dialogService.Show<MemberDialog>(_loc["_Create_New_Member"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true, FullWidth = true, FullScreen = true }, parameters: new() { { "FormModel", new MemberInsertRequest() }, { "FunctionName", "RegisterAdmin" } }).Result;
        if (checkInsert.Canceled) return;

        var dialogresponseData = checkInsert.Data.CastModel<MemberInsertRequest>();
        if (dialogresponseData is null) return;

        dialogresponseData.ActivateMember = true;
        var serviceResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Member/RegisterAdmin", dialogresponseData.ToJsonString());
        if (!serviceResponse.Success)
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Create_New_Member"]}", Severity.Error);
            return;
        }

        var parsedCreateMember = serviceResponse.ResponseData.CastModel<PublicActionResponse>();
        if (parsedCreateMember is null || !parsedCreateMember.SuccessAction || parsedCreateMember.ErrorException is not null)
        {
            _snackbar.Add($"{_loc[$"_ERROR_{(parsedCreateMember?.ErrorException?.ErrorCode ?? "NULLDATA")}"]} {_loc["_Create_New_Member"]}", Severity.Error);
            return;
        }

        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Create_New_Member"]}", Severity.Success);

        _listResponse ??= new();
        _listResponse.Members ??= new();
        int currentInsertedMemberID = -1;
        if (int.TryParse(parsedCreateMember.ObjectID, out int _insertedMemberID)) currentInsertedMemberID = _insertedMemberID;

        _listResponse.Members.Add(new() { Activate = true, Active = true, Givenname = dialogresponseData.GivenName, CellphoneNumber = dialogresponseData.CellphoneNumber, CityID = dialogresponseData.CityID, PolicyID = dialogresponseData.PolicyID ?? 0, CityName = dialogresponseData.CityName, CountryCode = dialogresponseData.CountryCode, CountryID = dialogresponseData.CountryID, Email = dialogresponseData.Email, Gender = dialogresponseData.Gender, MemberID = currentInsertedMemberID, PostalCode = dialogresponseData.PostalCode, RegisterDate = DateTime.Now, Surname = dialogresponseData.Surname, Username = dialogresponseData.Username });
    }

    private async Task ChangeActiveStatus(MemberListFinalResponse.MemberDetail memberDetail)
    {
        MemberChangeStatusRequest changeRequest = new() { MemberID = memberDetail.MemberID };
        var changeResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Member/ChangeStatusAdmin", changeRequest.ToJsonString());
        if (changeResponse is null || !changeResponse.Success || changeResponse.ErrorException is not null)
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Activat_Member"]}", Severity.Error);
            return;
        }

        var parsedchangeResponse = changeResponse.ResponseData.CastModel<PublicActionResponse>
                ();
        if (parsedchangeResponse.SuccessAction)
        {
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Active_DeActive"]}", Severity.Success);
            memberDetail.Active = !memberDetail.Active;
            return;
        }
        else if (parsedchangeResponse.ErrorException != null && !string.IsNullOrEmpty(parsedchangeResponse.ErrorException.ErrorCode))
        {
            _snackbar.Add($"{_loc[$"_ERROR_{parsedchangeResponse.ErrorException.ErrorCode}"]} {_loc["_Active_DeActive"]}", Severity.Error);
            return;
        }

    }

    private async Task ActivateMember(MemberListFinalResponse.MemberDetail memberDetail)
    {
        MemberActivateAdminRequest changeRequest = new() { MemberID = memberDetail.MemberID };
        var changeResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Member/ActivateAdmin", changeRequest.ToJsonString());
        if (changeResponse is null || !changeResponse.Success || changeResponse.ErrorException is not null)
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Activat_Member"]}", Severity.Error);
            return;
        }

        var parsedchangeResponse = changeResponse.ResponseData.CastModel<PublicActionResponse>
            ();
        if (parsedchangeResponse is null || !parsedchangeResponse.SuccessAction || parsedchangeResponse.ErrorException is not null)
        {
            _snackbar.Add($"{_loc[$"_ERROR_{(parsedchangeResponse?.ErrorException?.ErrorCode ?? "NULLDATA")}"]} {_loc["_Activat_Member"]}", Severity.Error);
            return;
        }

        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Activat_Member"]}", Severity.Success);
        memberDetail.Activate = true;
    }

    #endregion

    #region EditSection
    private async Task EditCurrentMember(MemberListFinalResponse.MemberDetail requestModel)
    {
        var checkInsert = await _dialogService.Show<MemberDialog>(_loc["_Member_Edit"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true, FullWidth = true, FullScreen = true }, parameters: new() { { "FormModel", requestModel.CastModel<MemberInsertRequest>() }, { "FunctionName", "EditAdmin" } }).Result;
        if (checkInsert.Canceled) return;

        var dialogresponseData = checkInsert.Data.CastModel<MemberInsertRequest>();
        if (dialogresponseData is null) return;

        var serviceResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Member/EditAdmin", dialogresponseData.ToJsonString());
        if (!serviceResponse.Success)
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Member_Edit"]}", Severity.Error);
            return;
        }

        var parsedServciceResponse = serviceResponse.ResponseData.CastModel<PublicActionResponse>();
        if (parsedServciceResponse is null || !parsedServciceResponse.SuccessAction || parsedServciceResponse.ErrorException is not null)
        {
            _snackbar.Add($"{_loc[$"_ERROR_{(parsedServciceResponse?.ErrorException?.ErrorCode ?? "NULLDATA")}"]} {_loc["_Member_Edit"]}", Severity.Error);
            return;
        }

        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Member_Edit"]}", Severity.Success);

        var memberData = (from i in _listResponse.Members where i.MemberID == (dialogresponseData.MemberID ?? 0) select i).FirstOrDefault();
        if (memberData != null)
        {
            memberData.Username = dialogresponseData.Username;
            memberData.Email = dialogresponseData.Email;
            memberData.CellphoneNumber = dialogresponseData.CellphoneNumber;
            memberData.CountryCode = dialogresponseData.CountryCode;
            memberData.Gender = dialogresponseData.Gender;
            memberData.Givenname = dialogresponseData.GivenName;
            memberData.Surname = dialogresponseData.Surname;
            memberData.PolicyID = dialogresponseData.PolicyID.Value;
        }
    }
    #endregion

    #region ActivateLink
    private async Task SendActivateLink(int memberID)
    {
        if ((await _dialogService.Show<ConfirmDialog>(_loc["_AreYouSure"], new DialogOptions { CloseOnEscapeKey = true, CloseButton = true, MaxWidth = MaxWidth.Small, Position = DialogPosition.Center }).Result).Canceled)
            return;

        var activationResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Member/SendActivateLinkAdmin", new MemberActivateSendLinkRequest() { CurrentMemberID = memberID }.ToJsonString());
        if (activationResponse.Success)
        {
            var parsedResponse = activationResponse.ResponseData.CastModel<PublicActionResponse>
                ();
            if (parsedResponse.SuccessAction)
            {
                _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Send_Activate_Link"]}", Severity.Success);
                return;
            }
            else if (parsedResponse.ErrorException != null && !string.IsNullOrEmpty(parsedResponse.ErrorException.ErrorCode))
            {
                _snackbar.Add($"{_loc[$"_ERROR_{parsedResponse.ErrorException.ErrorCode}"]} {_loc["_Send_Activate_Link"]}", Severity.Error);
                return;
            }
        }
        _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Send_Activate_Link"]}", Severity.Error);
    }
    #endregion

    #region ChangeMemberPassword
    private void ChangeMemberPassword(int memberID) =>
        _dialogService.Show<MemberPasswordDialog>(_loc["_NewPassword"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true }, parameters: new() { { "CurrentMemberID", memberID } });

    #endregion

    #region InsertNewMemberContact
    private void InsertNewMemberContact(int memberID) =>
        _dialogService.Show<ContactMemberDialog>(_loc["_Member_Insert_New_Address"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true }, parameters: new() { { "ContactModel", new MemberContactRequest() { CurrentMemberID = memberID } }, { "FunctionName", "ContactInsertAdmin" } });

    #endregion

    #region ViewAllContacts
    private async Task ViewAllContacts(int memberID)
    {
        var fetchContacts = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Member/ContactListAdmin", new MemberContactListRequest { CurrentMemberID = memberID }.ToJsonString());
        if (fetchContacts.Success)
        {
            var contactList = (fetchContacts.ResponseData.CastModel<MemberContactListResponse>().Contacts).CastModel<List<MemberContactRequest>>();
            _dialogService.Show<MemberContactListAdminDialog>(_loc["_Member_Profile_Addresses"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true, FullScreen = true }, parameters: new() { { "Addresses", contactList } });
        }
        else
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Member_Profile_Addresses"]}", Severity.Error);
        }
    }
    #endregion

}
