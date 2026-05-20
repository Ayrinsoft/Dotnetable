using Dotnetable.Admin.Models.DTO.Member;
using Dotnetable.Admin.Models.DTO.Place;
using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Member.Manage;

public partial class MemberForm
{
    [Inject] private IStringLocalizer<Resources.Resource> _loc { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }
    [Inject] private ISnackbar _snackbar { get; set; }

    [Parameter] public EventCallback<MemberInsertRequest> OnSubmitObject { get; set; }
    [Parameter] public MemberInsertRequest FormModel { get; set; }
    [Parameter] public string FunctionName { get; set; }

    //private byte _selectedCountryID = 0;
    private string _confirmPassword = "";

    protected override void OnInitialized()
    {
        if (FormModel is not null && !string.IsNullOrEmpty(FormModel.Username)) FormModel.Password = _confirmPassword = "QWEqwe@123";

        FormModel ??= new MemberInsertRequest();
    }

    //private void BindCities(CountryDetailResponse selectedCountry)
    //{
    //    _selectedCountryID = selectedCountry.CountryID;
    //}

    private void UpdateSelectedPolicy(PolicyListOnInsertMemberResponse selectedPolicy)
    {
        FormModel.PolicyID = selectedPolicy.PolicyID;
        StateHasChanged();
    }

    //private void UpdateCityItem(CityDetailResponse selectedCity)
    //{
    //    FormModel.CityID = selectedCity.CityID;
    //    FormModel.CityName = selectedCity.Title;
    //}

    private async Task OnSubmitForm()
    {
        if (FormModel.Password != _confirmPassword) return;
        if (FunctionName == "RegisterAdmin")
        {
            if (await InsertMember())
                await OnSubmitObject.InvokeAsync(FormModel);
        }
        else
        {
            if (await UpdateMember())
            {
                await OnSubmitObject.InvokeAsync(FormModel);
            }
        }        
    }


    private async Task<bool> InsertMember()
    {
        FormModel.ActivateMember = true;
        var serviceResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Member/RegisterAdmin", FormModel.ToJsonString());
        if (!serviceResponse.Success)
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Create_New_Member"]}", Severity.Error);
            return false;
        }

        var parsedCreateMember = serviceResponse.ResponseData.CastModel<PublicActionResponse>();
        if (parsedCreateMember is null || !parsedCreateMember.SuccessAction || parsedCreateMember.ErrorException is not null)
        {
            _snackbar.Add($"{_loc[$"_ERROR_{(parsedCreateMember?.ErrorException?.ErrorCode ?? "NULLDATA")}"]} {_loc["_Create_New_Member"]}", Severity.Error);
            return false;
        }

        if (int.TryParse(parsedCreateMember.ObjectID, out int _insertedMemberID)) FormModel.MemberID = _insertedMemberID;
        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Create_New_Member"]}", Severity.Success);
        return true;
    }

    private async Task<bool> UpdateMember()
    {
        var serviceResponse = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Member/EditAdmin", FormModel.ToJsonString());
        if (!serviceResponse.Success)
        {
            _snackbar.Add($"{_loc["_FailedAction"]} {_loc["_Member_Edit"]}", Severity.Error);
            return false;
        }

        var parsedServciceResponse = serviceResponse.ResponseData.CastModel<PublicActionResponse>();
        if (parsedServciceResponse is null || !parsedServciceResponse.SuccessAction || parsedServciceResponse.ErrorException is not null)
        {
            _snackbar.Add($"{_loc[$"_ERROR_{(parsedServciceResponse?.ErrorException?.ErrorCode ?? "NULLDATA")}"]} {_loc["_Member_Edit"]}", Severity.Error);
            return false;
        }

        _snackbar.Add($"{_loc["_SuccessAction"]} {_loc["_Member_Edit"]}", Severity.Success);
        return true;
    }


}
