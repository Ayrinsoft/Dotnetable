using Dotnetable.Application.DTOs;
using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

/// <summary>
/// Public side of the admin-built dynamic forms/surveys. A form is reachable on its own page
/// (<c>/form/{slug}</c>) and can also be embedded in any CMS page/post via the <c>[form:ID]</c>
/// shortcode — both render through <see cref="FormHtmlRenderer"/> and post back here.
/// </summary>
public class FormController : Controller
{
    private readonly ApiClient _api;

    public FormController(ApiClient api) => _api = api;

    [HttpGet("/form/{slug}")]
    public async Task<IActionResult> View(string slug, CancellationToken ct = default)
    {
        var form = await _api.GetFormBySlugAsync(slug, ct);
        if (form is null) return NotFound();

        ViewData["Title"] = form.Title;
        ViewData["Form"] = form;
        ViewData["Submitted"] = TempData[SubmittedKey(form.FormID)] as string;
        ViewData["Error"] = TempData[ErrorKey(form.FormID)] as string;

        // After a successful survey submission, show the live aggregate results (when enabled).
        if (form.ShowResults && ViewData["Submitted"] is not null)
            ViewData["Results"] = await _api.GetFormResultsAsync(form.FormID, ct);

        return View();
    }

    /// <summary>Receives the browser post of a rendered form (standalone page or shortcode embed),
    /// forwards it to the API for validation/storage, and redirects back with the outcome.
    /// Anti-forgery is intentionally skipped: submissions are anonymous by design and fully
    /// validated server-side (the API also records the sender IP).</summary>
    [HttpPost("/form/submit/{id:int}")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Submit(int id, CancellationToken ct = default)
    {
        var request = new FormSubmissionRequest();
        foreach (var (key, values) in Request.Form)
        {
            if (!key.StartsWith("field_", StringComparison.Ordinal) ||
                !int.TryParse(key["field_".Length..], out var fieldId))
                continue;

            request.Answers.Add(new FormAnswerInput
            {
                FormFieldID = fieldId,
                Values = values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!).ToList(),
            });
        }

        var result = await _api.SubmitFormAsync(id, request, ct);

        if (result.Ok)
            TempData[SubmittedKey(id)] = string.IsNullOrWhiteSpace(result.Message)
                ? "Thank you! Your response has been recorded."
                : result.Message;
        else
            TempData[ErrorKey(id)] = result.Message ?? "Could not submit the form. Please try again.";

        // Shortcode embeds pass the page they live on so the visitor lands back there.
        var returnUrl = Request.Form["returnUrl"].ToString();
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl + $"#form-{id}");

        var form = await _api.GetFormByIdAsync(id, ct);
        return form is null ? NotFound() : Redirect($"/form/{form.Slug}");
    }

    private static string SubmittedKey(int formId) => $"form_submitted_{formId}";
    private static string ErrorKey(int formId) => $"form_error_{formId}";
}
