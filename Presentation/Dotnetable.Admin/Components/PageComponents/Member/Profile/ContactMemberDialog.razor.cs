using Dotnetable.Shared.DTO.Member;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Member.Profile
{
    public partial class ContactMemberDialog
    {
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
        [Parameter] public string FunctionName { get; set; }
        [Parameter] public MemberContactRequest ContactModel { get; set; }


        private void OnEditContactComplete(MemberContactRequest insertedContact)
        {
            if (insertedContact is not null)
                MudDialog.Close(insertedContact);
        }

    }
}
