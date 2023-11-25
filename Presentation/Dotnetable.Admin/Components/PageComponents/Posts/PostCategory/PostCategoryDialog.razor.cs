using Dotnetable.Shared.DTO.Post;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Posts.PostCategory;

public partial class PostCategoryDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public string FunctionName { get; set; }
    [Parameter] public PostCategoryInsertRequest FormModel { get; set; }
    [Parameter] public string DefaultLanguageCode { get; set; }


    private void OnSubmitModel(PostCategoryInsertRequest formModel)
    {
        if (formModel is not null)
            MudDialog.Close(formModel);
    }
}
