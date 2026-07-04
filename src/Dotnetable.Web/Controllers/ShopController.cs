using Dotnetable.Application.DTOs;
using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

/// <summary>Public product catalog: category/brand listing and product detail, backed by the API.</summary>
public class ShopController : Controller
{
    private const int PageSize = 12;

    private readonly ApiClient _api;

    public ShopController(ApiClient api) => _api = api;

    public sealed record ShopListView(
        IReadOnlyList<ProductSummaryDto> Products,
        IReadOnlyList<ProductCategoryDto> Categories,
        int Page, int TotalCount, int PageSize,
        string? Category, string? Brand, string? Search)
    {
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public async Task<IActionResult> Index(
        int page = 1, string? category = null, string? brand = null, string? search = null, CancellationToken ct = default)
    {
        var lang = CurrentLang();
        var result = await _api.GetProductsAsync(category, brand, search, page: page, pageSize: PageSize, lang: lang, ct: ct);
        var categories = await _api.GetProductCategoryTreeAsync(lang, ct);

        return View(new ShopListView(result.Items, categories, page, result.TotalCount, PageSize, category, brand, search));
    }

    public Task<IActionResult> Category(string slug, int page = 1, CancellationToken ct = default) =>
        Index(page, category: slug, ct: ct);

    public async Task<IActionResult> Product(string slug, CancellationToken ct = default)
    {
        var product = await _api.GetProductAsync(slug, CurrentLang(), ct: ct);
        if (product is null) return NotFound();

        var reviews = await _api.GetProductReviewsAsync(product.ProductID, ct: ct);
        var questions = await _api.GetProductQuestionsAsync(product.ProductID, ct: ct);
        ViewBag.Reviews = reviews;
        ViewBag.Questions = questions;
        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int variantId, int quantity = 1, CancellationToken ct = default)
    {
        var result = await _api.AddToCartAsync(variantId, quantity, ct);
        return result.Ok ? RedirectToAction("Index", "Cart") : BadRequest(new { message = result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> SubmitReview(int productId, byte rating, string? title, string body, CancellationToken ct = default)
    {
        var result = await _api.SubmitReviewAsync(productId, rating, title, body, ct);
        return result.Ok ? Ok(new { success = true }) : BadRequest(new { message = result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> AskQuestion(int productId, string body, CancellationToken ct = default)
    {
        var result = await _api.AskQuestionAsync(productId, body, ct);
        return result.Ok ? Ok(new { success = true }) : BadRequest(new { message = result.Message });
    }

    private string? CurrentLang()
    {
        var lang = Request.Cookies["lang"];
        return string.IsNullOrWhiteSpace(lang) ? null : lang;
    }
}
