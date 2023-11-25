using Dotnetable.Shared.DTO.Post;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Dotnetable.Admin.Components.PageComponents.Posts.PostCategory;

public partial class PostCategoryLanguageForm
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Parameter] public PostCategoryUpdateOtherLanguageRequest FormModel { get; set; }
    [Parameter] public EventCallback<PostCategoryUpdateOtherLanguageRequest> OnSubmitObject { get; set; }

    private async Task OnSubmitForm() => await OnSubmitObject.InvokeAsync(FormModel);
}
