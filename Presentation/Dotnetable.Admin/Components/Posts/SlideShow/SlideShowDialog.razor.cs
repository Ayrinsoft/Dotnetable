using Dotnetable.Shared.DTO.Post;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dotnetable.Admin.Components.Posts.SlideShow;

public partial class SlideShowDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }

    [Parameter] public SlideShowInsertRequest FormModel { get; set; }

    private void SubmitedSlideShow(SlideShowInsertRequest requestModel)
    {
        if (requestModel is not null)
            MudDialog.Close(requestModel);
    }

}
