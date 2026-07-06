using System.Text;
using System.Text.Encodings.Web;
using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Web.Services;

/// <summary>
/// Renders an admin-built dynamic form/survey (<see cref="FormDto"/>) to Bootstrap HTML. Shared by
/// the standalone /form/{slug} page and the <c>[form:ID]</c> shortcode, so a form looks identical
/// wherever the admin embeds it. The generated form posts to <c>/form/submit/{id}</c>.
/// </summary>
public static class FormHtmlRenderer
{
    public static string Render(FormDto? form, string? returnUrl = null)
    {
        if (form is null || form.Fields.Count == 0) return string.Empty;

        var h = HtmlEncoder.Default;
        var sb = new StringBuilder();

        sb.Append($"<div class=\"dn-form card border-0 shadow-sm\" id=\"form-{form.FormID}\"><div class=\"card-body p-4\">");
        sb.Append($"<h3 class=\"h4 fw-bold mb-1\">{h.Encode(form.Title)}</h3>");
        if (!string.IsNullOrWhiteSpace(form.Description))
            sb.Append($"<p class=\"text-muted mb-4\">{h.Encode(form.Description)}</p>");

        if (!form.IsOpen)
        {
            sb.Append("<div class=\"alert alert-secondary mb-0\">This form is not accepting responses right now.</div>");
            sb.Append("</div></div>");
            return sb.ToString();
        }

        sb.Append($"<form method=\"post\" action=\"/form/submit/{form.FormID}\">");
        if (!string.IsNullOrWhiteSpace(returnUrl))
            sb.Append($"<input type=\"hidden\" name=\"returnUrl\" value=\"{h.Encode(returnUrl)}\" />");

        foreach (var field in form.Fields)
        {
            var name = $"field_{field.FormFieldID}";
            var required = field.IsRequired ? " required" : "";
            var requiredMark = field.IsRequired ? " <span class=\"text-danger\">*</span>" : "";
            var placeholder = string.IsNullOrWhiteSpace(field.Placeholder) ? "" : $" placeholder=\"{h.Encode(field.Placeholder)}\"";

            if (field.FieldType == FormFieldType.SectionTitle)
            {
                sb.Append($"<h4 class=\"h5 fw-bold mt-4 mb-3\">{h.Encode(field.Label)}</h4>");
                if (!string.IsNullOrWhiteSpace(field.HelpText))
                    sb.Append($"<p class=\"text-muted\">{h.Encode(field.HelpText)}</p>");
                continue;
            }

            sb.Append("<div class=\"mb-3\">");
            sb.Append($"<label class=\"form-label fw-semibold\">{h.Encode(field.Label)}{requiredMark}</label>");

            switch (field.FieldType)
            {
                case FormFieldType.TextArea:
                    sb.Append($"<textarea class=\"form-control\" name=\"{name}\" rows=\"4\"{placeholder}{MaxLength(field)}{required}></textarea>");
                    break;

                case FormFieldType.Number:
                    sb.Append($"<input type=\"number\" class=\"form-control\" name=\"{name}\"{placeholder}{MinMax(field)}{required} />");
                    break;

                case FormFieldType.Email:
                    sb.Append($"<input type=\"email\" class=\"form-control\" name=\"{name}\"{placeholder}{required} />");
                    break;

                case FormFieldType.Phone:
                    sb.Append($"<input type=\"tel\" class=\"form-control\" name=\"{name}\"{placeholder}{required} />");
                    break;

                case FormFieldType.Date:
                    sb.Append($"<input type=\"date\" class=\"form-control\" name=\"{name}\"{required} />");
                    break;

                case FormFieldType.Select:
                    sb.Append($"<select class=\"form-select\" name=\"{name}\"{required}>");
                    sb.Append("<option value=\"\">—</option>");
                    foreach (var option in field.Options)
                        sb.Append($"<option value=\"{h.Encode(option.Value)}\">{h.Encode(option.Label)}</option>");
                    sb.Append("</select>");
                    break;

                case FormFieldType.Radio:
                case FormFieldType.Checkbox:
                {
                    var type = field.FieldType == FormFieldType.Radio ? "radio" : "checkbox";
                    // Only radios can carry `required` safely (checkbox required would force every box).
                    var itemRequired = field.FieldType == FormFieldType.Radio ? required : "";
                    foreach (var option in field.Options)
                    {
                        var id = $"{name}_{option.FormFieldOptionID}";
                        sb.Append("<div class=\"form-check\">");
                        sb.Append($"<input class=\"form-check-input\" type=\"{type}\" name=\"{name}\" id=\"{id}\" value=\"{h.Encode(option.Value)}\"{itemRequired} />");
                        sb.Append($"<label class=\"form-check-label\" for=\"{id}\">{h.Encode(option.Label)}</label>");
                        sb.Append("</div>");
                    }
                    break;
                }

                case FormFieldType.Rating:
                {
                    var min = field.MinValue ?? 1;
                    var max = field.MaxValue ?? 5;
                    sb.Append("<div class=\"d-flex flex-wrap gap-3\">");
                    for (int score = min; score <= max; score++)
                    {
                        var id = $"{name}_{score}";
                        sb.Append("<div class=\"form-check form-check-inline\">");
                        sb.Append($"<input class=\"form-check-input\" type=\"radio\" name=\"{name}\" id=\"{id}\" value=\"{score}\"{required} />");
                        sb.Append($"<label class=\"form-check-label\" for=\"{id}\">{score}</label>");
                        sb.Append("</div>");
                    }
                    sb.Append("</div>");
                    break;
                }

                case FormFieldType.YesNo:
                    sb.Append("<div class=\"form-check form-check-inline\">");
                    sb.Append($"<input class=\"form-check-input\" type=\"radio\" name=\"{name}\" id=\"{name}_yes\" value=\"yes\"{required} />");
                    sb.Append($"<label class=\"form-check-label\" for=\"{name}_yes\">Yes</label></div>");
                    sb.Append("<div class=\"form-check form-check-inline\">");
                    sb.Append($"<input class=\"form-check-input\" type=\"radio\" name=\"{name}\" id=\"{name}_no\" value=\"no\"{required} />");
                    sb.Append($"<label class=\"form-check-label\" for=\"{name}_no\">No</label></div>");
                    break;

                default: // Text
                    sb.Append($"<input type=\"text\" class=\"form-control\" name=\"{name}\"{placeholder}{MaxLength(field)}{required} />");
                    break;
            }

            if (!string.IsNullOrWhiteSpace(field.HelpText))
                sb.Append($"<div class=\"form-text\">{h.Encode(field.HelpText)}</div>");
            sb.Append("</div>");
        }

        var submitText = string.IsNullOrWhiteSpace(form.SubmitButtonText) ? "Submit" : form.SubmitButtonText;
        sb.Append($"<button type=\"submit\" class=\"btn btn-primary btn-lg\">{h.Encode(submitText)}</button>");
        sb.Append("</form></div></div>");
        return sb.ToString();
    }

    /// <summary>Aggregate survey results as Bootstrap progress bars (public ShowResults view).</summary>
    public static string RenderResults(FormPublicResultsDto? results)
    {
        if (results is null) return string.Empty;

        var h = HtmlEncoder.Default;
        var sb = new StringBuilder();
        sb.Append("<div class=\"dn-form-results card border-0 shadow-sm mt-4\"><div class=\"card-body p-4\">");
        sb.Append($"<h3 class=\"h5 fw-bold mb-1\">{h.Encode(results.Title)}</h3>");
        sb.Append($"<p class=\"text-muted\">{results.ResponseCount} responses</p>");

        foreach (var field in results.Fields)
        {
            sb.Append($"<h4 class=\"h6 fw-semibold mt-4\">{h.Encode(field.Label)}");
            if (field.Average is double avg)
                sb.Append($" <span class=\"text-muted fw-normal\">(avg {avg})</span>");
            sb.Append("</h4>");

            foreach (var option in field.OptionCounts)
            {
                sb.Append("<div class=\"d-flex justify-content-between small\">");
                sb.Append($"<span>{h.Encode(option.Label)}</span><span class=\"text-muted\">{option.Count} ({option.Percent}%)</span></div>");
                sb.Append("<div class=\"progress mb-2\" style=\"height:8px\">");
                sb.Append($"<div class=\"progress-bar\" role=\"progressbar\" style=\"width:{option.Percent}%\"></div></div>");
            }
        }

        sb.Append("</div></div>");
        return sb.ToString();
    }

    private static string MaxLength(FormFieldDto field) =>
        field.MaxValue is int max ? $" maxlength=\"{max}\"" : "";

    private static string MinMax(FormFieldDto field)
    {
        var sb = new StringBuilder();
        if (field.MinValue is int min) sb.Append($" min=\"{min}\"");
        if (field.MaxValue is int max) sb.Append($" max=\"{max}\"");
        return sb.ToString();
    }
}
