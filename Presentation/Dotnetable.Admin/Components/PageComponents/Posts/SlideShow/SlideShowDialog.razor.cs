using Dotnetable.Shared.DTO.Post;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Posts.SlideShow;

public partial class SlideShowDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }

    [Parameter] public SlideShowInsertRequest FormModel { get; set; }

    private void SubmitedSlideShow(SlideShowInsertRequest requestModel)
    {
        if (requestModel is not null)
            MudDialog.Close(requestModel);
    }

}
