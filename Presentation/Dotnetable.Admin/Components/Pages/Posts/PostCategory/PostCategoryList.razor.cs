using Dotnetable.Service;
using Dotnetable.Shared.DTO.Post;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;

namespace Dotnetable.Admin.Components.Pages.Posts.PostCategory;

public partial class PostCategoryList
{
    [Inject] private PostService _post { get; set; }
    [Inject] private IMemoryCache _mmc { get; set; }

    [Parameter] public string CssClass { get; set; }
    [Parameter] public EventCallback<int> SelectedLastPostCategoryID { get; set; }
    [Parameter] public int? DefaultPostCategoryID { get; set; }
    [Parameter] public string LanguageCode { get; set; }

    private List<PostCategoryListResponse.PostCategoryDetail> _cachedPostCategory { get; set; }

    protected async override Task OnInitializedAsync()
    {
        if (!_mmc.TryGetValue("PostCategoryList", out List<PostCategoryListResponse.PostCategoryDetail> _postCategoryList))
        {
            var fetchPostCategory = await _post.PostCategoryList();
            if (fetchPostCategory.ErrorException is null)
            {
                _postCategoryList = fetchPostCategory.PostCategories;
                _mmc.Set("PostCategoryList", _postCategoryList, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(3)));
            }
        }
        _cachedPostCategory = _postCategoryList;
    }
}
