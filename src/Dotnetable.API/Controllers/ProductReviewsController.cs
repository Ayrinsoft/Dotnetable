using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Dotnetable.Hosting;

namespace Dotnetable.API.Controllers;

/// <summary>Public reads (approved reviews) + client-authenticated writes for product reviews.</summary>
[Route("api/products/{productId:int}/reviews")]
[EnableRateLimiting(RateLimiting.PublicWritePolicy)]
public class ProductReviewsController : BaseController
{
    private readonly IProductReviewService _reviews;
    private readonly IWebsiteService _websiteService;

    public ProductReviewsController(IProductReviewService reviews, IWebsiteService websiteService)
    {
        _reviews = reviews;
        _websiteService = websiteService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetApproved(int productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default) =>
        Ok(await _reviews.GetApprovedAsync(productId, new GridQuery { PageIndex = page, PageSize = pageSize }, ct));

    public sealed record SubmitReviewRequest(int? VariantId, byte Rating, string? Title, string Body, string? ProsJson, string? ConsJson);

    [HttpPost]
    [Authorize(Policy = RoleKeys.ClientReview)]
    public async Task<IActionResult> Submit(int productId, [FromBody] SubmitReviewRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var review = await _reviews.SubmitAsync(
            website.WebsiteID, CurrentClientId, productId, request.VariantId, request.Rating,
            request.Title, request.Body, request.ProsJson, request.ConsJson, ct);
        return Ok(new { review.ProductReviewID });
    }

    [HttpPost("~/api/reviews/{reviewId:int}/like")]
    [Authorize(Policy = RoleKeys.ClientReview)]
    public async Task<IActionResult> Like(int reviewId, CancellationToken ct = default)
    {
        await _reviews.LikeAsync(reviewId, ct);
        return Ok();
    }

    [HttpPost("~/api/reviews/{reviewId:int}/dislike")]
    [Authorize(Policy = RoleKeys.ClientReview)]
    public async Task<IActionResult> Dislike(int reviewId, CancellationToken ct = default)
    {
        await _reviews.DislikeAsync(reviewId, ct);
        return Ok();
    }

    private int CurrentClientId =>
        int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var id)
            ? id
            : throw new InvalidOperationException("Token does not carry a client id.");
}
