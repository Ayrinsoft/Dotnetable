using Dotnetable.Shared.DTO.Message;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Dotnetable.Admin.Components.PageComponents.Messages.EmailSetting;

public partial class EmailSettingForm
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }

    [Parameter] public EmailPanelUpdateRequest FormModel { get; set; }
    [Parameter] public string DefaultLanguageCode { get; set; }
    [Parameter] public EventCallback<EmailPanelUpdateRequest> OnSubmitObject { get; set; }
    [Parameter] public string FunctionName { get; set; }

    private bool _formEnableSSL = false;
    private bool _formDefaultEmail = false;

    protected override void OnInitialized()
    {
        FormModel ??= new();
        _formEnableSSL = FormModel.EnableSSL ?? false;
        _formDefaultEmail = FormModel.DefaultEmail ?? false;
    }

    private async Task OnSubmitForm()
    {
        FormModel.EnableSSL = _formEnableSSL;
        FormModel.DefaultEmail = _formDefaultEmail;
        await OnSubmitObject.InvokeAsync(FormModel);
        StateHasChanged();
    }


}
