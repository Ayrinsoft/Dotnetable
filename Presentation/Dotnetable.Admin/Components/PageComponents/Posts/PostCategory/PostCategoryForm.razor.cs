using Dotnetable.Shared.DTO.Post;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Dotnetable.Admin.Components.PageComponents.Posts.PostCategory;

public partial class PostCategoryForm
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Parameter] public PostCategoryInsertRequest FormModel { get; set; }
    [Parameter] public EventCallback<PostCategoryInsertRequest> OnSubmitObject { get; set; }


    protected override void OnInitialized() => FormModel ??= new() { Priority = 1 };
    private async Task OnSubmitForm() => await OnSubmitObject.InvokeAsync(FormModel);

}
