using Dotnetable.Shared.DTO.Post;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dotnetable.Admin.Components.Posts.PostCategory;

public partial class PostCategoryLanguageDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }

    [Parameter] public PostCategoryUpdateOtherLanguageRequest FormModel { get; set; }

    private void SubmitDialog(PostCategoryUpdateOtherLanguageRequest requestModel) => MudDialog.Close(requestModel);
}
