using Dotnetable.Shared.DTO.Post;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Posts.PostCategory;

public partial class PostCategoryLanguageDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }

    [Parameter] public PostCategoryUpdateOtherLanguageRequest FormModel { get; set; }

    private void SubmitDialog(PostCategoryUpdateOtherLanguageRequest requestModel) => MudDialog.Close(requestModel);
}
