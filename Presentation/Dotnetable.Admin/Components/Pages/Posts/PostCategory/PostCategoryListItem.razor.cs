using Dotnetable.Shared.DTO.Post;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Dotnetable.Admin.Components.Pages.Posts.PostCategory;

public partial class PostCategoryListItem
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }

    [Parameter] public int? ParentID { get; set; }
    [Parameter] public int? DefaultPostCategoryID { get; set; }
    [Parameter] public int? DefaultPostCategoryParentID { get; set; }
    [Parameter] public List<PostCategoryListResponse.PostCategoryDetail> PostCategoryList { get; set; }
    [Parameter] public string CssClass { get; set; }
    [Parameter] public EventCallback<int> SelectedLastPostCategoryID { get; set; }
    [Parameter] public string LanguageCode { get; set; }

    private int? _selectedPostCategoryID { get; set; }
    private bool _showChilds { get; set; }
    private int? _tempParentID { get; set; }

    private List<PostCategoryListResponse.PostCategoryDetail> _currentPostCategories { get; set; }

    protected async override Task OnInitializedAsync()
    {
        ParentID ??= 0;
        _showChilds = false;
        _selectedPostCategoryID = -1;
        _currentPostCategories = PostCategoryList.Where(i => i.ParentID == ParentID && (string.IsNullOrEmpty(LanguageCode) || i.LanguageCode == LanguageCode)).ToList();

        if (PostCategoryList != null && DefaultPostCategoryID.HasValue)
        {
            _tempParentID = LastParentID(DefaultPostCategoryID.Value);
            if (_tempParentID > 0)
            {
                _selectedPostCategoryID = _tempParentID;
                await GenerateChilds();
            }
        }
    }

    private async Task OnChangePostCatSelect(ChangeEventArgs e)
    {
        _selectedPostCategoryID = Convert.ToInt32(e.Value);
        await GenerateChilds();
    }

    private async Task GenerateChilds()
    {
        if (!PostCategoryList.Any(i => i.ParentID == _selectedPostCategoryID)) _showChilds = false;
        else _showChilds = true;
        await SelectedLastPostCategoryID.InvokeAsync(_selectedPostCategoryID.Value);
        StateHasChanged();
    }
    private int LastParentID(int postCatID)
    {
        var fetchParentPostCat = (from i in PostCategoryList where i.PostCategoryID == postCatID select new { i.ParentID }).FirstOrDefault();
        if (fetchParentPostCat is null || fetchParentPostCat.ParentID is null || fetchParentPostCat.ParentID == 0 || fetchParentPostCat.ParentID == DefaultPostCategoryParentID)
            return postCatID;
        else if (fetchParentPostCat.ParentID == null || fetchParentPostCat.ParentID == 0)
            return 0;
        else
            return LastParentID(fetchParentPostCat.ParentID.Value);
    }
}
