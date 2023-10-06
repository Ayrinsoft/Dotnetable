using Dotnetable.Shared.DTO.Post;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Dotnetable.Admin.Components.Posts.SlideShow;

public partial class SlideshowLanguageForm
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }

    [Parameter] public SlideShowInsertLanguageRequest FormModel { get; set; }
    [Parameter] public EventCallback<SlideShowInsertLanguageRequest> OnSubmitObject { get; set; }

    private Dictionary<string, string> _slideShowSettings = new();
    private string _settingKey = "";
    private string _settingValue = "";

    protected override void OnInitialized()
    {
        FormModel.LanguageCode = FormModel.LanguageCode.ToUpper();
        if (!string.IsNullOrEmpty(FormModel.SettingsArray) && FormModel.SettingsArray != "{}") _slideShowSettings = FormModel.SettingsArray.JsonToObject<Dictionary<string, string>>();
    }

    private async Task OnSubmitForm()
    {
        if (_slideShowSettings is not null && _slideShowSettings.Count > 0) FormModel.SettingsArray = _slideShowSettings.ToJsonString();
        await OnSubmitObject.InvokeAsync(FormModel);
    }

    private void AddNewSetting()
    {
        if (!string.IsNullOrEmpty(_settingKey) && _settingKey != "" && !string.IsNullOrEmpty(_settingValue) && _settingValue != "")
        {
            _slideShowSettings.Add(_settingKey, _settingValue);
            _settingKey = _settingValue = "";
        }
    }

    private void RemoveSetting(string settingKey) => _slideShowSettings.Remove(settingKey);
}
