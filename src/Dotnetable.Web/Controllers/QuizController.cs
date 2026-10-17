using Dotnetable.Application.DTOs;
using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

/// <summary>Public timed tests. A menu item of type Quiz or QuizCategory lands here.</summary>
public class QuizController : Controller
{
    private readonly ApiClient _api;

    public QuizController(ApiClient api) => _api = api;

    [HttpGet("/quiz/category/{id:int}")]
    public async Task<IActionResult> Category(int id, CancellationToken ct = default)
    {
        var category = await _api.GetQuizCategoryAsync(id, Lang(), ct);
        if (category is null) return NotFound();
        ViewData["Title"] = category.Title;
        return View(category);
    }

    [HttpGet("/quiz/{id:int}")]
    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        var quiz = await _api.GetQuizAsync(id, Lang(), ct);
        if (quiz is null) return NotFound();
        ViewData["Title"] = quiz.Title;
        return View(quiz);
    }

    [HttpPost("/quiz/{id:int}/start")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Start(int id, CancellationToken ct = default)
    {
        var attempt = await _api.StartQuizAsync(id, Lang(), ct);
        if (attempt is null) return Redirect($"/quiz/{id}");
        return Redirect($"/quiz/attempt/{attempt.Token}");
    }

    [HttpGet("/quiz/attempt/{token}")]
    public async Task<IActionResult> Attempt(string token, CancellationToken ct = default)
    {
        var attempt = await _api.GetQuizAttemptAsync(token, ct);
        if (attempt is null) return NotFound();
        ViewData["Title"] = attempt.Title;
        ViewData["Notice"] = TempData["QuizNotice"] as string;
        ViewData["Error"] = TempData["QuizError"] as string;
        return View(attempt);
    }

    [HttpPost("/quiz/attempt/{token}")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Submit(string token, CancellationToken ct = default)
    {
        var request = new QuizSubmitRequest();
        var ids = new HashSet<int>();
        foreach (var key in Request.Form.Keys)
        {
            if (key.StartsWith("q_", StringComparison.Ordinal) && int.TryParse(key[2..], out var questionId))
                ids.Add(questionId);
            if (key.StartsWith("t_", StringComparison.Ordinal) && int.TryParse(key[2..], out var textId))
                ids.Add(textId);
        }

        foreach (var id in ids)
        {
            var options = new List<int>();
            if (Request.Form.TryGetValue($"q_{id}", out var selected))
            {
                foreach (var value in selected)
                {
                    if (int.TryParse(value, out var optionId)) options.Add(optionId);
                }
            }

            request.Answers.Add(new QuizAnswerInput
            {
                QuizQuestionID = id,
                OptionIds = options,
                Text = Request.Form[$"t_{id}"].ToString(),
            });
        }

        await _api.SubmitQuizAsync(token, request, ct);
        return Redirect($"/quiz/attempt/{token}");
    }

    [HttpPost("/quiz/attempt/{token}/review")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Review(string token, CancellationToken ct = default)
    {
        var payOnline = Request.Form["payOnline"] == "1";
        int.TryParse(Request.Form["gatewayId"], out var gatewayId);
        var returnUrl = $"{Request.Scheme}://{Request.Host}/quiz/pay/return/{token}";
        var result = await _api.RequestQuizReviewAsync(token, new QuizPrivateReviewRequest
        {
            FullName = Request.Form["fullName"].ToString(),
            Phone = Request.Form["phone"].ToString(),
            Email = Request.Form["email"].ToString(),
            PayOnline = payOnline,
            GatewayId = gatewayId > 0 ? gatewayId : null,
            ReturnUrl = returnUrl,
        }, ct);

        if (!string.IsNullOrWhiteSpace(result.RedirectUrl))
            return Redirect(result.RedirectUrl);

        TempData[result.Ok ? "QuizNotice" : "QuizError"] = result.Message
            ?? (result.Ok ? "Your request was recorded." : "Could not send the request.");
        return Redirect($"/quiz/attempt/{token}");
    }

    [HttpGet("/quiz/pay/return/{token}")]
    [HttpPost("/quiz/pay/return/{token}")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> PayReturn(string token, CancellationToken ct = default)
    {
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in Request.Query)
            fields[key] = value.ToString();
        if (Request.HasFormContentType)
        {
            foreach (var (key, value) in Request.Form)
                fields[key] = value.ToString();
        }

        var result = await _api.CompleteQuizPaymentAsync(token, fields, ct);
        TempData[result.Ok ? "QuizNotice" : "QuizError"] = result.Ok
            ? "Payment confirmed. The specialist will send your private answer."
            : (result.Message ?? "The payment was not confirmed.");
        return Redirect($"/quiz/attempt/{token}");
    }

    private string? Lang()
    {
        var lang = Request.Cookies["lang"];
        return string.IsNullOrWhiteSpace(lang) ? null : lang;
    }
}
