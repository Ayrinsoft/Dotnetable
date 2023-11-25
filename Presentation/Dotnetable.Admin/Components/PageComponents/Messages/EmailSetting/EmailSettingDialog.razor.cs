using Dotnetable.Shared.DTO.Message;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Messages.EmailSetting;

public partial class EmailSettingDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public EmailPanelUpdateRequest FormModel { get; set; }


    private void OnSubmitModel(EmailPanelUpdateRequest formModel)
    {
        if (formModel is not null)
            MudDialog.Close(formModel);
    }
}
