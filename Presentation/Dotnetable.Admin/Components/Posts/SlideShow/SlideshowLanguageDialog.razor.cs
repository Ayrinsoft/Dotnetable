using Dotnetable.Shared.DTO.Post;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dotnetable.Admin.Components.Posts.SlideShow;

public partial class SlideshowLanguageDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }

    [Parameter] public SlideShowInsertLanguageRequest FormModel { get; set; }
    
    private void SubmitDialog(SlideShowInsertLanguageRequest requestModel) => MudDialog.Close(requestModel);
    
}
