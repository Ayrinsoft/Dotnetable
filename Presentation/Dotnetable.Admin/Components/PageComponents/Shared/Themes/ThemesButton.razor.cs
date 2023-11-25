using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Dotnetable.Admin.Components.PageComponents.Shared.Themes;

public partial class ThemesButton
{
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [EditorRequired][Parameter] public bool RTLLayout { get; set; }

}