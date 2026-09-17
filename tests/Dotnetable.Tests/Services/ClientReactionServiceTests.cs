using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class ClientReactionServiceTests : IDisposable
{
    private const int SiteId = 1;
    private const int OtherSiteId = 2;
    private const int Sara = 30;
    private const int Ali = 31;
    private const int PostId = 10;
    private const int DraftPostId = 11;
    private const int PageId = 20;
    private const int ProductId = 40;
    private const int OtherSiteProductId = 41;
    private const int DefaultVariantId = 400;
    private const int SecondVariantId = 401;

    private readonly RelationalTestDb _db;
    private readonly AppDbContext _context;
    private readonly ClientReactionService _service;
    private readonly WishlistService _wishlist;
    private readonly PostService _posts;

    public ClientReactionServiceTests()
    {
        _db = new RelationalTestDb();
        _context = _db.NewContext();
        var factory = new TestDbContextFactory(_db.Options);
        _service = new ClientReactionService(factory);
        _wishlist = new WishlistService(factory);
        _posts = new PostService(factory);
        Seed();
    }

    public void Dispose()
    {
        _context.Dispose();
        _db.Dispose();
    }

    private void Seed()
    {
        _context.WebsiteClients.AddRange(
            new WebsiteClient { WebsiteClientID = Sara, WebsiteID = SiteId, Active = true, Givenname = "Sara" },
            new WebsiteClient { WebsiteClientID = Ali, WebsiteID = SiteId, Active = true, Givenname = "Ali" });
        _context.PostTypes.Add(new PostType { PostTypeID = 1, WebsiteID = SiteId, Name = "Blog", Slug = "blog" });
        _context.Posts.AddRange(
            new Post { PostID = PostId, WebsiteID = SiteId, PostTypeID = 1, Slug = "hello", Title = "Hello", Status = PostService.PublishedStatus, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Post { PostID = DraftPostId, WebsiteID = SiteId, PostTypeID = 1, Slug = "draft", Title = "Draft", Status = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        _context.Pages.Add(new Page { PageID = PageId, WebsiteID = SiteId, Slug = "about", Title = "About", Status = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        _context.Products.AddRange(
            new Product { ProductID = ProductId, WebsiteID = SiteId, Slug = "mug", Title = "Mug", Status = ProductService.PublishedStatus, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Product { ProductID = OtherSiteProductId, WebsiteID = OtherSiteId, Slug = "cup", Title = "Cup", Status = ProductService.PublishedStatus, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        _context.ProductVariants.AddRange(
            new ProductVariant { ProductVariantID = SecondVariantId, WebsiteID = SiteId, ProductID = ProductId, Sku = "MUG-B", Title = "Blue", IsActive = true },
            new ProductVariant { ProductVariantID = DefaultVariantId, WebsiteID = SiteId, ProductID = ProductId, Sku = "MUG-R", Title = "Red", IsDefault = true, IsActive = true });
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
    }

    private Task<int> LikeCountAsync(int postId) =>
        _context.Posts.AsNoTracking().Where(p => p.PostID == postId).Select(p => p.LikeCount).SingleAsync();

    private Task<int> FavoriteCountAsync() =>
        _context.Products.AsNoTracking().Where(p => p.ProductID == ProductId).Select(p => p.FavoriteCount).SingleAsync();

    // ── Likes are counted once per client ───────────────────────────

    [Fact]
    public async Task Like_Twice_By_Same_Client_Counts_Once()
    {
        await _service.SetAsync(SiteId, Sara, ReactionTargetType.Post, PostId, ReactionType.Like, true);
        var state = await _service.SetAsync(SiteId, Sara, ReactionTargetType.Post, PostId, ReactionType.Like, true);

        state!.LikeCount.Should().Be(1);
        state.Liked.Should().BeTrue();
        (await LikeCountAsync(PostId)).Should().Be(1);
        (await _context.ClientReactions.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Likes_From_Different_Clients_Add_Up_And_Unlike_Removes_Only_Own()
    {
        await _service.SetAsync(SiteId, Sara, ReactionTargetType.Post, PostId, ReactionType.Like, true);
        await _service.SetAsync(SiteId, Ali, ReactionTargetType.Post, PostId, ReactionType.Like, true);
        (await LikeCountAsync(PostId)).Should().Be(2);

        var state = await _service.SetAsync(SiteId, Sara, ReactionTargetType.Post, PostId, ReactionType.Like, false);
        state!.LikeCount.Should().Be(1);
        state.Liked.Should().BeFalse();

        // Unliking again (or never having liked) must not push the counter down.
        await _service.SetAsync(SiteId, Sara, ReactionTargetType.Post, PostId, ReactionType.Like, false);
        (await LikeCountAsync(PostId)).Should().Be(1);

        (await _service.GetStateAsync(SiteId, ReactionTargetType.Post, PostId, Ali))!.Liked.Should().BeTrue();
    }

    [Fact]
    public async Task Bookmark_Does_Not_Touch_The_Like_Counter()
    {
        var state = await _service.SetAsync(SiteId, Sara, ReactionTargetType.Page, PageId, ReactionType.Bookmark, true);

        state!.Bookmarked.Should().BeTrue();
        state.Liked.Should().BeFalse();
        state.LikeCount.Should().Be(0);
    }

    [Fact]
    public async Task Page_Like_Is_Counted_On_The_Page()
    {
        var state = await _service.SetAsync(SiteId, Sara, ReactionTargetType.Page, PageId, ReactionType.Like, true);

        state!.LikeCount.Should().Be(1);
        (await _context.Pages.AsNoTracking().SingleAsync(p => p.PageID == PageId)).LikeCount.Should().Be(1);
    }

    [Fact]
    public async Task Unpublished_Or_Foreign_Targets_Are_Rejected()
    {
        (await _service.SetAsync(SiteId, Sara, ReactionTargetType.Post, DraftPostId, ReactionType.Like, true)).Should().BeNull();
        (await _service.SetAsync(SiteId, Sara, ReactionTargetType.Product, OtherSiteProductId, ReactionType.Bookmark, true)).Should().BeNull();
        (await _service.GetStateAsync(SiteId, ReactionTargetType.Post, 999, null)).Should().BeNull();
        (await _context.ClientReactions.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Anonymous_State_Has_Count_But_No_Flags()
    {
        await _service.SetAsync(SiteId, Sara, ReactionTargetType.Post, PostId, ReactionType.Like, true);

        var state = await _service.GetStateAsync(SiteId, ReactionTargetType.Post, PostId, null);

        state!.LikeCount.Should().Be(1);
        state.Liked.Should().BeFalse();
        state.Bookmarked.Should().BeFalse();
    }

    // ── Product favorite ⇄ wishlist ─────────────────────────────────

    [Fact]
    public async Task Favoriting_A_Product_Adds_Its_Default_Variant_To_The_Wishlist()
    {
        var state = await _service.SetAsync(SiteId, Sara, ReactionTargetType.Product, ProductId, ReactionType.Like, true);

        state!.LikeCount.Should().Be(1);
        var items = await _wishlist.GetItemsAsync(Sara);
        items.Select(i => i.ProductVariantID).Should().Equal(DefaultVariantId);
    }

    [Fact]
    public async Task Unfavoriting_Removes_Every_Variant_Of_The_Product_From_The_Wishlist()
    {
        await _wishlist.AddItemAsync(SiteId, Sara, DefaultVariantId);
        await _wishlist.AddItemAsync(SiteId, Sara, SecondVariantId);

        var state = await _service.SetAsync(SiteId, Sara, ReactionTargetType.Product, ProductId, ReactionType.Like, false);

        state!.LikeCount.Should().Be(0);
        (await _wishlist.GetItemsAsync(Sara)).Should().BeEmpty();
    }

    [Fact]
    public async Task Wishlist_Counts_A_Product_Once_However_Many_Variants_Are_Saved()
    {
        await _wishlist.AddItemAsync(SiteId, Sara, DefaultVariantId);
        await _wishlist.AddItemAsync(SiteId, Sara, SecondVariantId);
        (await FavoriteCountAsync()).Should().Be(1);

        await _wishlist.RemoveItemAsync(Sara, DefaultVariantId);
        (await FavoriteCountAsync()).Should().Be(1, "another variant of the product is still saved");

        await _wishlist.RemoveItemAsync(Sara, SecondVariantId);
        (await FavoriteCountAsync()).Should().Be(0);
        (await _service.GetStateAsync(SiteId, ReactionTargetType.Product, ProductId, Sara))!.Liked.Should().BeFalse();
    }

    [Fact]
    public async Task Wishlist_Ignores_A_Variant_Of_Another_Website()
    {
        _context.ProductVariants.Add(new ProductVariant { ProductVariantID = 500, WebsiteID = OtherSiteId, ProductID = OtherSiteProductId, Sku = "CUP", Title = "Cup", IsActive = true });
        await _context.SaveChangesAsync();

        await _wishlist.AddItemAsync(SiteId, Sara, 500);

        (await _wishlist.GetItemsAsync(Sara)).Should().BeEmpty();
    }

    // ── Lists and cleanup ───────────────────────────────────────────

    [Fact]
    public async Task Bookmark_List_Is_Newest_First_And_Filterable_And_Hides_Unpublished()
    {
        await _service.SetAsync(SiteId, Sara, ReactionTargetType.Post, PostId, ReactionType.Bookmark, true);
        await _service.SetAsync(SiteId, Sara, ReactionTargetType.Page, PageId, ReactionType.Bookmark, true);
        await _service.SetAsync(SiteId, Sara, ReactionTargetType.Product, ProductId, ReactionType.Bookmark, true);
        await _service.SetAsync(SiteId, Ali, ReactionTargetType.Post, PostId, ReactionType.Bookmark, true);

        var all = await _service.GetListAsync(SiteId, Sara, ReactionType.Bookmark, null, 1, 20);
        all.TotalCount.Should().Be(3);
        all.Items.Select(i => i.Title).Should().BeEquivalentTo(["Hello", "About", "Mug"]);

        var posts = await _service.GetListAsync(SiteId, Sara, ReactionType.Bookmark, ReactionTargetType.Post, 1, 20);
        posts.Items.Should().ContainSingle().Which.Slug.Should().Be("hello");

        await _context.Posts.Where(p => p.PostID == PostId).ExecuteUpdateAsync(s => s.SetProperty(p => p.IsActive, false));
        (await _service.GetListAsync(SiteId, Sara, ReactionType.Bookmark, null, 1, 20)).TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Deleting_A_Post_Deletes_Its_Reactions()
    {
        await _service.SetAsync(SiteId, Sara, ReactionTargetType.Post, PostId, ReactionType.Like, true);
        await _service.SetAsync(SiteId, Ali, ReactionTargetType.Post, PostId, ReactionType.Bookmark, true);
        await _service.SetAsync(SiteId, Ali, ReactionTargetType.Page, PageId, ReactionType.Bookmark, true);

        await _posts.DeleteAsync(PostId);

        (await _context.ClientReactions.AsNoTracking().Select(r => r.TargetType).ToListAsync())
            .Should().Equal((byte)ReactionTargetType.Page);
    }

    [Fact]
    public async Task Post_Payload_Carries_The_Like_Count()
    {
        await _service.SetAsync(SiteId, Sara, ReactionTargetType.Post, PostId, ReactionType.Like, true);

        var post = await _posts.GetBySlugAsync(SiteId, "hello");

        post!.LikeCount.Should().Be(1);
    }
}
