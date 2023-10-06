using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.DTO.Post;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;

namespace Dotnetable.Admin.Pages.Posts.PostCategory;

public partial class PostCategoryList
{
    [Inject] private IHttpServices _httpService { get; set; }
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
            var fetchPostCategory = await _httpService.CallServiceObjAsync(HttpMethod.Get, true, "Post/PostCategoryList");
            if (fetchPostCategory.Success)
            {
                _postCategoryList = fetchPostCategory.ResponseData.CastModel<PostCategoryListResponse>().PostCategories;
                _mmc.Set("PostCategoryList", _postCategoryList, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(3)));
            }
        }
        _cachedPostCategory = _postCategoryList;
    }
}
