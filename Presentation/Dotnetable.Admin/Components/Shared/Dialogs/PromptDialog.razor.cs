using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.Shared.Dialogs;

public partial class PromptDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }

    [Parameter] public string ColumnTitle { get; set; }
    [Parameter] public string DefaultValue { get; set; }

    private string _returnedValue = "";

    protected override void OnInitialized()
    {
        if (!string.IsNullOrEmpty(DefaultValue)) _returnedValue = DefaultValue;
    }
    void Submit()
    {
        if (!string.IsNullOrEmpty(_returnedValue) && _returnedValue != "")
            MudDialog.Close(_returnedValue);
    }
    void Cancel() => MudDialog.Cancel();

    private void SubmitForm(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
            Submit();
    }


}
