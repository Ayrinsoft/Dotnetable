using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Dotnetable.Hosting;

namespace Dotnetable.API.Controllers;

/// <summary>Public reads (approved Q&amp;A) + client-authenticated writes.</summary>
[Route("api/products/{productId:int}/questions")]
[EnableRateLimiting(RateLimiting.PublicWritePolicy)]
public class ProductQuestionsController : BaseController
{
    private readonly IProductQuestionService _questions;
    private readonly IWebsiteService _websiteService;

    public ProductQuestionsController(IProductQuestionService questions, IWebsiteService websiteService)
    {
        _questions = questions;
        _websiteService = websiteService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetApproved(int productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default) =>
        Ok(await _questions.GetApprovedAsync(productId, new GridQuery { PageIndex = page, PageSize = pageSize }, ct));

    public sealed record AskRequest(string Body);

    [HttpPost]
    [Authorize(Policy = RoleKeys.ClientReview)]
    public async Task<IActionResult> Ask(int productId, [FromBody] AskRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var question = await _questions.AskAsync(website.WebsiteID, CurrentClientId, productId, request.Body, ct);
        return Ok(new { question.ProductQuestionID });
    }

    public sealed record AnswerRequest(string Body);

    [HttpPost("~/api/questions/{questionId:int}/answers")]
    [Authorize(Policy = RoleKeys.ClientReview)]
    public async Task<IActionResult> Answer(int questionId, [FromBody] AnswerRequest request, CancellationToken ct = default)
    {
        var answer = await _questions.AnswerAsync(questionId, CurrentClientId, null, request.Body, ct);
        return Ok(new { answer.ProductAnswerID });
    }

    [HttpPost("~/api/answers/{answerId:int}/like")]
    [Authorize(Policy = RoleKeys.ClientReview)]
    public async Task<IActionResult> LikeAnswer(int answerId, CancellationToken ct = default)
    {
        await _questions.LikeAnswerAsync(answerId, ct);
        return Ok();
    }

    private int CurrentClientId =>
        int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var id)
            ? id
            : throw new InvalidOperationException("Token does not carry a client id.");
}
