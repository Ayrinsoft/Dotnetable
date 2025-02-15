using Dotnetable.Shared.DTO.Member;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Member.Manage;

public partial class MemberDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    [Parameter] public string FunctionName { get; set; }
    [Parameter] public MemberInsertRequest FormModel { get; set; }


    private void OnSubmitObject(MemberInsertRequest responseFormModel)
    {
        if (responseFormModel is not null)
            MudDialog.Close(responseFormModel);
    }
}
