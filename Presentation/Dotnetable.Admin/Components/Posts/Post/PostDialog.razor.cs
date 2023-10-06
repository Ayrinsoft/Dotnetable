using Dotnetable.Shared.DTO.Post;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dotnetable.Admin.Components.Posts.Post;

public partial class PostDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public string FunctionName { get; set; }
    [Parameter] public PostUpdateRequest FormModel { get; set; }
    [Parameter] public string DefaultLanguageCode { get; set; }


    private void OnSubmitModel(PostUpdateRequest formModel)
    {
        if (formModel is not null)
            MudDialog.Close(formModel);
    }
}
