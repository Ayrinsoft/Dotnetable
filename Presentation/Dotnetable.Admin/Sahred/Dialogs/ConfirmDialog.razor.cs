using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Sahred.Dialogs;

public partial class ConfirmDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Inject] private IStringLocalizer<Shared.Resources.Resource> _loc { get; set; }


    void Submit() => MudDialog.Close(DialogResult.Ok(true));
    void Cancel() => MudDialog.Cancel();
}
