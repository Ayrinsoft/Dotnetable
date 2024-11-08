using Dotnetable.Admin.Components.PageComponents.Member.Manage;
using Dotnetable.Admin.Components.PageComponents.Member.Profile;
using Dotnetable.Admin.Components.Shared.Dialogs;
using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
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
    [Inject] private MemberService _member { get; set; }
    [Inject] private Tools _tools { get; set; }
    [Inject] private IDialogService _dialogService { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    private MemberListRequest _listRequest { get; set; }
    private MemberListFinalResponse _listResponse { get; set; }
    private GridViewHeaderParameters _gridHeaderParams { get; set; }
    int memberID = -1;

    #region Grid
    protected async override Task OnInitializedAsync()
    {
        memberID = await _tools.GetRequesterMemberID();
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
            Username = _gridHeaderParams.HeaderItems.FirstOrDefault(i => i.ColumnName == nameof(MemberListFinalResponse.MemberDetail.Username)).SearchText
        };
    }

    private async Task FetchGrid()
    {
        _listResponse = await _member.MemberList(_listRequest);
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
        var serviceResponse = await _member.Register(dialogresponseData);
        if (!serviceResponse.SuccessAction)
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Create_New_Member"]}", Severity.Error);
            return;
        }

        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Create_New_Member"]}", Severity.Success);

        _listResponse ??= new();
        _listResponse.Members ??= new();
        int currentInsertedMemberID = -1;
        if (int.TryParse(serviceResponse.ObjectID, out int _insertedMemberID)) currentInsertedMemberID = _insertedMemberID;

        _listResponse.Members.Add(new() { Activate = true, Active = true, Givenname = dialogresponseData.GivenName, CellphoneNumber = dialogresponseData.CellphoneNumber, CityID = dialogresponseData.CityID, PolicyID = dialogresponseData.PolicyID ?? 0, CityName = dialogresponseData.CityName, CountryCode = dialogresponseData.CountryCode, CountryID = dialogresponseData.CountryID, Email = dialogresponseData.Email, Gender = dialogresponseData.Gender, MemberID = currentInsertedMemberID, PostalCode = dialogresponseData.PostalCode, RegisterDate = DateTime.Now, Surname = dialogresponseData.Surname, Username = dialogresponseData.Username });
    }

    private async Task ChangeActiveStatus(MemberListFinalResponse.MemberDetail memberDetail)
    {
        MemberChangeStatusRequest changeRequest = new() { MemberID = memberDetail.MemberID };
        var changeResponse = await _member.ChangeStatus(changeRequest);
        if (changeResponse is null || !changeResponse.SuccessAction || changeResponse.ErrorException is not null)
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Activat_Member"]}", Severity.Error);
            return;
        }

        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Active_DeActive"]}", Severity.Success);
        memberDetail.Active = !memberDetail.Active;
        return;
    }

    private async Task ActivateMember(MemberListFinalResponse.MemberDetail memberDetail)
    {
        MemberActivateAdminRequest changeRequest = new() { MemberID = memberDetail.MemberID };
        var changeResponse = await _member.ActivateAdmin(changeRequest);
        if (changeResponse is null || !changeResponse.SuccessAction || changeResponse.ErrorException is not null)
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Activat_Member"]}", Severity.Error);
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

        var dialogresponseData = checkInsert.Data.CastModel<MemberEditRequest>();
        if (dialogresponseData is null) return;

        var serviceResponse = await _member.Edit(dialogresponseData);
        if (!serviceResponse.SuccessAction)
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Member_Edit"]}", Severity.Error);
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
            memberData.Givenname = dialogresponseData.Givenname;
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

        var activationResponse = await _member.SendActivateLink(new() { CurrentMemberID = memberID });
        if (activationResponse.SuccessAction)
        {
            _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Send_Activate_Link"]}", Severity.Success);
            return;
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
        var fetchContacts = await _member.ContactList(new() { CurrentMemberID = memberID});
        if (fetchContacts.ErrorException is null)
        {
            var contactList = fetchContacts.Contacts.CastModel<List<MemberContactRequest>>();
            _dialogService.Show<MemberContactListAdminDialog>(_loc["_Member_Profile_Addresses"], options: new DialogOptions { CloseButton = true, CloseOnEscapeKey = true, FullScreen = true }, parameters: new() { { "Addresses", contactList } });
        }
        else
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Member_Profile_Addresses"]}", Severity.Error);
        }
    }
    #endregion

}
