using Dotnetable.Admin.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Dotnetable.Admin.Components.Pages.Messages;

public partial class ContactUsMessages
{

    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }
}
