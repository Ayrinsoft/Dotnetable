using System.Text;
using System.Text.Json;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class FormService : IFormService
{
    /// <summary>Free-text sample size shown per field in the aggregate report.</summary>
    private const int TextSampleCount = 10;

    private readonly AppDbContext _context;
    private readonly IAdminNotificationService _notifications;

    public FormService(AppDbContext context, IAdminNotificationService notifications)
    {
        _context = context;
        _notifications = notifications;
    }

    // ── Admin: forms ────────────────────────────────────────────────

    public async Task<List<Form>> GetFormsAsync(int? websiteId, CancellationToken ct = default)
    {
        var q = _context.Forms.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(f => f.WebsiteID == wid);
        return await q
            .Select(f => new Form
            {
                FormID = f.FormID,
                WebsiteID = f.WebsiteID,
                Title = f.Title,
                Slug = f.Slug,
                FormType = f.FormType,
                RequireLogin = f.RequireLogin,
                AllowMultipleSubmissions = f.AllowMultipleSubmissions,
                ShowResults = f.ShowResults,
                StartAt = f.StartAt,
                EndAt = f.EndAt,
                IsActive = f.IsActive,
                CreatedAt = f.CreatedAt,
                FormFields = f.FormFields.Where(x => x.IsActive).ToList(),
                FormResponses = f.FormResponses.Select(r => new FormResponse { FormResponseID = r.FormResponseID }).ToList(),
            })
            .OrderByDescending(f => f.FormID)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<Form>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.Forms.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(f => f.WebsiteID == wid);

        if (query.GetSearch(nameof(Form.Title)) is string title)
            q = q.Where(f => f.Title.Contains(title));
        if (query.GetSearch(nameof(Form.Slug)) is string slug)
            q = q.Where(f => f.Slug.Contains(slug));
        if (query.GetSearch(nameof(Form.FormType)) is string typeStr && byte.TryParse(typeStr, out var formType))
            q = q.Where(f => f.FormType == formType);
        if (query.GetSearch(nameof(Form.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(f => f.IsActive == isActive);
        if (query.GetSearch(nameof(Form.FormID)) is string idStr && int.TryParse(idStr, out var formId))
            q = q.Where(f => f.FormID == formId);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Form.FormID), fallbackDescending: true)
            .Skip(query.Skip).Take(query.Take)
            .Select(f => new Form
            {
                FormID = f.FormID,
                WebsiteID = f.WebsiteID,
                Title = f.Title,
                Slug = f.Slug,
                FormType = f.FormType,
                RequireLogin = f.RequireLogin,
                AllowMultipleSubmissions = f.AllowMultipleSubmissions,
                ShowResults = f.ShowResults,
                StartAt = f.StartAt,
                EndAt = f.EndAt,
                IsActive = f.IsActive,
                CreatedAt = f.CreatedAt,
                FormFields = f.FormFields.Where(x => x.IsActive).ToList(),
                FormResponses = f.FormResponses.Select(r => new FormResponse { FormResponseID = r.FormResponseID }).ToList(),
            })
            .ToListAsync(ct);

        return new PagedResult<Form> { Items = items, TotalCount = total };
    }

    public async Task<Form?> GetFormAsync(int formId, CancellationToken ct = default) =>
        await _context.Forms.AsNoTracking()
            .Include(f => f.FormFields.OrderBy(x => x.SortOrder).ThenBy(x => x.FormFieldID))
                .ThenInclude(x => x.FormFieldOptions.OrderBy(o => o.SortOrder).ThenBy(o => o.FormFieldOptionID))
            .FirstOrDefaultAsync(f => f.FormID == formId, ct);

    public async Task<Form> CreateFormAsync(Form form, CancellationToken ct = default)
    {
        form.Slug = NormalizeSlug(form.Slug, form.Title);
        form.CreatedAt = DateTime.UtcNow;
        _context.Forms.Add(form);
        await _context.SaveChangesAsync(ct);
        return form;
    }

    public async Task UpdateFormAsync(Form form, CancellationToken ct = default)
    {
        form.Slug = NormalizeSlug(form.Slug, form.Title);
        _context.Forms.Update(form);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteFormAsync(int formId, CancellationToken ct = default)
    {
        var form = await _context.Forms
            .Include(f => f.FormFields).ThenInclude(x => x.FormFieldOptions)
            .Include(f => f.FormResponses).ThenInclude(r => r.FormResponseValues)
            .FirstOrDefaultAsync(f => f.FormID == formId, ct);
        if (form is null) return;

        _context.FormResponseValues.RemoveRange(form.FormResponses.SelectMany(r => r.FormResponseValues));
        _context.FormResponses.RemoveRange(form.FormResponses);
        _context.FormFieldOptions.RemoveRange(form.FormFields.SelectMany(x => x.FormFieldOptions));
        _context.FormFields.RemoveRange(form.FormFields);
        _context.Forms.Remove(form);
        await _context.SaveChangesAsync(ct);
    }

    // ── Admin: fields ───────────────────────────────────────────────

    public async Task<List<FormField>> GetFieldsAsync(int formId, CancellationToken ct = default) =>
        await _context.FormFields.AsNoTracking()
            .Include(x => x.FormFieldOptions.OrderBy(o => o.SortOrder).ThenBy(o => o.FormFieldOptionID))
            .Where(x => x.FormID == formId)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.FormFieldID)
            .ToListAsync(ct);

    public async Task<FormField> SaveFieldAsync(FormField field, List<FormFieldOption> options, CancellationToken ct = default)
    {
        if (field.FormFieldID == 0)
        {
            if (field.SortOrder == 0)
            {
                var maxOrder = await _context.FormFields
                    .Where(x => x.FormID == field.FormID)
                    .MaxAsync(x => (int?)x.SortOrder, ct) ?? 0;
                field.SortOrder = maxOrder + 1;
            }
            _context.FormFields.Add(field);
        }
        else
        {
            _context.FormFields.Update(field);
            var existing = await _context.FormFieldOptions
                .Where(o => o.FormFieldID == field.FormFieldID)
                .ToListAsync(ct);
            _context.FormFieldOptions.RemoveRange(existing);
        }

        for (int i = 0; i < options.Count; i++)
        {
            options[i].FormFieldOptionID = 0;
            options[i].FormField = field;
            options[i].SortOrder = i;
            _context.FormFieldOptions.Add(options[i]);
        }

        await _context.SaveChangesAsync(ct);
        return field;
    }

    public async Task DeleteFieldAsync(int fieldId, CancellationToken ct = default)
    {
        var field = await _context.FormFields
            .Include(x => x.FormFieldOptions)
            .Include(x => x.FormResponseValues)
            .FirstOrDefaultAsync(x => x.FormFieldID == fieldId, ct);
        if (field is null) return;

        _context.FormResponseValues.RemoveRange(field.FormResponseValues);
        _context.FormFieldOptions.RemoveRange(field.FormFieldOptions);
        _context.FormFields.Remove(field);
        await _context.SaveChangesAsync(ct);
    }

    public async Task ReorderFieldsAsync(int formId, List<int> orderedFieldIds, CancellationToken ct = default)
    {
        var fields = await _context.FormFields.Where(x => x.FormID == formId).ToListAsync(ct);
        for (int i = 0; i < orderedFieldIds.Count; i++)
        {
            var field = fields.FirstOrDefault(x => x.FormFieldID == orderedFieldIds[i]);
            if (field is not null) field.SortOrder = i;
        }
        await _context.SaveChangesAsync(ct);
    }

    // ── Admin: reporting ────────────────────────────────────────────

    public async Task<FormReportDto?> GetReportAsync(int formId, CancellationToken ct = default)
    {
        var form = await GetFormAsync(formId, ct);
        if (form is null) return null;

        var responseStats = await _context.FormResponses.AsNoTracking()
            .Where(r => r.FormID == formId)
            .GroupBy(r => 1)
            .Select(g => new { Count = g.Count(), First = g.Min(r => r.SubmittedAt), Last = g.Max(r => r.SubmittedAt) })
            .FirstOrDefaultAsync(ct);

        var values = await _context.FormResponseValues.AsNoTracking()
            .Where(v => v.FormResponse.FormID == formId)
            .OrderByDescending(v => v.FormResponseID)
            .Select(v => new { v.FormFieldID, v.Value })
            .ToListAsync(ct);
        var valuesByField = values.GroupBy(v => v.FormFieldID)
            .ToDictionary(g => g.Key, g => g.Select(v => v.Value).ToList());

        return new FormReportDto
        {
            FormID = form.FormID,
            Title = form.Title,
            FormType = (FormType)form.FormType,
            ResponseCount = responseStats?.Count ?? 0,
            FirstResponseAt = responseStats?.First,
            LastResponseAt = responseStats?.Last,
            Fields = form.FormFields
                .Where(f => (FormFieldType)f.FieldType != FormFieldType.SectionTitle)
                .OrderBy(f => f.SortOrder).ThenBy(f => f.FormFieldID)
                .Select(f => BuildFieldReport(f, valuesByField.TryGetValue(f.FormFieldID, out var v) ? v : new List<string?>()))
                .ToList(),
        };
    }

    public async Task<PagedResult<FormResponseListItemDto>> GetResponsesAsync(int formId, int page, int pageSize, CancellationToken ct = default)
    {
        var q = _context.FormResponses.AsNoTracking().Where(r => r.FormID == formId);
        var total = await q.CountAsync(ct);

        var rows = await q
            .OrderByDescending(r => r.FormResponseID)
            .Skip((page < 1 ? 0 : page - 1) * pageSize).Take(pageSize)
            .Select(r => new
            {
                r.FormResponseID,
                r.SubmittedAt,
                r.SenderIPAddress,
                ClientName = r.WebsiteClient == null
                    ? null
                    : ((r.WebsiteClient.Givenname ?? "") + " " + (r.WebsiteClient.Surname ?? "")).Trim(),
                ClientEmail = r.WebsiteClient == null ? null : r.WebsiteClient.Email,
                Values = r.FormResponseValues.Select(v => new { v.FormFieldID, v.Value }).ToList(),
            })
            .ToListAsync(ct);

        return new PagedResult<FormResponseListItemDto>
        {
            TotalCount = total,
            Items = rows.Select(r => new FormResponseListItemDto
            {
                FormResponseID = r.FormResponseID,
                SubmittedAt = r.SubmittedAt,
                SenderIPAddress = r.SenderIPAddress,
                ClientDisplayName = string.IsNullOrWhiteSpace(r.ClientName) ? r.ClientEmail : r.ClientName,
                Answers = r.Values
                    .GroupBy(v => v.FormFieldID)
                    .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(v => DisplayValue(v.Value)))),
            }).ToList(),
        };
    }

    public async Task DeleteResponseAsync(int responseId, CancellationToken ct = default)
    {
        var response = await _context.FormResponses
            .Include(r => r.FormResponseValues)
            .FirstOrDefaultAsync(r => r.FormResponseID == responseId, ct);
        if (response is null) return;

        _context.FormResponseValues.RemoveRange(response.FormResponseValues);
        _context.FormResponses.Remove(response);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<byte[]> ExportResponsesExcelAsync(int formId, CancellationToken ct = default)
    {
        var form = await GetFormAsync(formId, ct)
            ?? throw new InvalidOperationException($"Form {formId} not found.");

        var fields = form.FormFields
            .Where(f => (FormFieldType)f.FieldType != FormFieldType.SectionTitle)
            .OrderBy(f => f.SortOrder).ThenBy(f => f.FormFieldID)
            .ToList();

        var responses = await _context.FormResponses.AsNoTracking()
            .Where(r => r.FormID == formId)
            .OrderBy(r => r.FormResponseID)
            .Select(r => new
            {
                r.FormResponseID,
                r.SubmittedAt,
                r.SenderIPAddress,
                ClientEmail = r.WebsiteClient == null ? null : r.WebsiteClient.Email,
                Values = r.FormResponseValues.Select(v => new { v.FormFieldID, v.Value }).ToList(),
            })
            .ToListAsync(ct);

        var headers = new List<string> { "ResponseID", "SubmittedAtUtc", "Client", "IPAddress" };
        headers.AddRange(fields.Select(f => f.Label));

        var dataRows = responses.Select(r =>
        {
            var byField = r.Values.GroupBy(v => v.FormFieldID)
                .ToDictionary(g => g.Key, g => string.Join("; ", g.Select(v => DisplayValue(v.Value))));
            var cells = new List<object?>
            {
                r.FormResponseID,
                r.SubmittedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                r.ClientEmail ?? "",
                r.SenderIPAddress,
            };
            cells.AddRange(fields.Select(f => byField.TryGetValue(f.FormFieldID, out var v) ? v : ""));
            return (IReadOnlyList<object?>)cells;
        });

        return ExcelWorkbook.Write("Responses", headers, dataRows);
    }

    // ── Public (API-facing) ─────────────────────────────────────────

    public async Task<FormDto?> GetPublicFormAsync(int websiteId, string slug, CancellationToken ct = default)
    {
        var form = await LoadPublicFormQuery(websiteId)
            .FirstOrDefaultAsync(f => f.Slug == slug, ct);
        return form is null ? null : ProjectPublic(form);
    }

    public async Task<FormDto?> GetPublicFormByIdAsync(int websiteId, int formId, CancellationToken ct = default)
    {
        var form = await LoadPublicFormQuery(websiteId)
            .FirstOrDefaultAsync(f => f.FormID == formId, ct);
        return form is null ? null : ProjectPublic(form);
    }

    public async Task<FormSubmissionResult> SubmitAsync(int websiteId, int formId, int? websiteClientId, string senderIp, FormSubmissionRequest request, CancellationToken ct = default)
    {
        var form = await _context.Forms.AsNoTracking()
            .Include(f => f.FormFields.Where(x => x.IsActive))
                .ThenInclude(x => x.FormFieldOptions)
            .FirstOrDefaultAsync(f => f.FormID == formId && f.WebsiteID == websiteId && f.IsActive, ct);
        if (form is null)
            return FormSubmissionResult.Fail("Form not found.");
        if (!IsOpen(form))
            return FormSubmissionResult.Fail("This form is not accepting responses right now.");
        if (form.RequireLogin && websiteClientId is null)
            return FormSubmissionResult.Fail("Please sign in to submit this form.");

        if (!form.AllowMultipleSubmissions && websiteClientId is int cid)
        {
            var already = await _context.FormResponses
                .AnyAsync(r => r.FormID == formId && r.WebsiteClientID == cid, ct);
            if (already)
                return FormSubmissionResult.Fail("You have already submitted this form.");
        }

        var answersByField = request.Answers
            .GroupBy(a => a.FormFieldID)
            .ToDictionary(g => g.Key, g => g.SelectMany(a => a.Values)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim())
                .ToList());

        var response = new FormResponse
        {
            FormID = form.FormID,
            WebsiteClientID = websiteClientId,
            SenderIPAddress = senderIp.Length > 45 ? senderIp[..45] : senderIp,
            SubmittedAt = DateTime.UtcNow,
        };

        foreach (var field in form.FormFields)
        {
            var type = (FormFieldType)field.FieldType;
            if (type == FormFieldType.SectionTitle) continue;

            answersByField.TryGetValue(field.FormFieldID, out var values);
            values ??= new List<string>();

            if (field.IsRequired && values.Count == 0)
                return FormSubmissionResult.Fail($"\"{field.Label}\" is required.");
            if (values.Count == 0) continue;

            var error = ValidateAnswer(field, type, values);
            if (error is not null)
                return FormSubmissionResult.Fail(error);

            response.FormResponseValues.Add(new FormResponseValue
            {
                FormFieldID = field.FormFieldID,
                Value = type == FormFieldType.Checkbox && values.Count > 1
                    ? JsonSerializer.Serialize(values)
                    : values[0],
            });
        }

        if (response.FormResponseValues.Count == 0)
            return FormSubmissionResult.Fail("The submission is empty.");

        _context.FormResponses.Add(response);
        await _context.SaveChangesAsync(ct);

        await _notifications.NotifySiteAdminsAsync(
            websiteId,
            AdminNotificationType.FormSubmission,
            "New form submission",
            $"Form \"{form.Title}\" received a new response.",
            $"/forms/{form.FormID}/report",
            response.FormResponseID,
            ct);

        return FormSubmissionResult.Success(form.SuccessMessage);
    }

    public async Task<FormPublicResultsDto?> GetPublicResultsAsync(int websiteId, int formId, CancellationToken ct = default)
    {
        var allowed = await _context.Forms.AsNoTracking()
            .AnyAsync(f => f.FormID == formId && f.WebsiteID == websiteId && f.IsActive && f.ShowResults, ct);
        if (!allowed) return null;

        var report = await GetReportAsync(formId, ct);
        if (report is null) return null;

        return new FormPublicResultsDto
        {
            FormID = report.FormID,
            Title = report.Title,
            ResponseCount = report.ResponseCount,
            // Free-text answers stay private; the public view only gets aggregates.
            Fields = report.Fields
                .Where(f => f.OptionCounts.Count > 0 || f.Average is not null)
                .Select(f => new FormFieldReportDto
                {
                    FormFieldID = f.FormFieldID,
                    Label = f.Label,
                    FieldType = f.FieldType,
                    AnswerCount = f.AnswerCount,
                    Average = f.Average,
                    OptionCounts = f.OptionCounts,
                })
                .ToList(),
        };
    }

    // ── Helpers ─────────────────────────────────────────────────────

    private IQueryable<Form> LoadPublicFormQuery(int websiteId) =>
        _context.Forms.AsNoTracking()
            .Where(f => f.WebsiteID == websiteId && f.IsActive)
            .Include(f => f.FormFields.Where(x => x.IsActive))
                .ThenInclude(x => x.FormFieldOptions);

    private static bool IsOpen(Form form)
    {
        var now = DateTime.UtcNow;
        if (form.StartAt is DateTime start && now < start) return false;
        if (form.EndAt is DateTime end && now > end) return false;
        return true;
    }

    private static FormDto ProjectPublic(Form form) => new()
    {
        FormID = form.FormID,
        Title = form.Title,
        Slug = form.Slug,
        Description = form.Description,
        FormType = (FormType)form.FormType,
        SubmitButtonText = form.SubmitButtonText,
        SuccessMessage = form.SuccessMessage,
        RequireLogin = form.RequireLogin,
        AllowMultipleSubmissions = form.AllowMultipleSubmissions,
        ShowResults = form.ShowResults,
        IsOpen = IsOpen(form),
        Fields = form.FormFields
            .OrderBy(x => x.SortOrder).ThenBy(x => x.FormFieldID)
            .Select(x => new FormFieldDto
            {
                FormFieldID = x.FormFieldID,
                Label = x.Label,
                FieldType = (FormFieldType)x.FieldType,
                Placeholder = x.Placeholder,
                HelpText = x.HelpText,
                IsRequired = x.IsRequired,
                MinValue = x.MinValue,
                MaxValue = x.MaxValue,
                Options = x.FormFieldOptions
                    .OrderBy(o => o.SortOrder).ThenBy(o => o.FormFieldOptionID)
                    .Select(o => new FormFieldOptionDto
                    {
                        FormFieldOptionID = o.FormFieldOptionID,
                        Label = o.Label,
                        Value = string.IsNullOrWhiteSpace(o.Value) ? o.Label : o.Value,
                    })
                    .ToList(),
            })
            .ToList(),
    };

    private static string? ValidateAnswer(FormField field, FormFieldType type, List<string> values)
    {
        switch (type)
        {
            case FormFieldType.Select:
            case FormFieldType.Radio:
            case FormFieldType.Checkbox:
            {
                var allowed = field.FormFieldOptions
                    .Select(o => string.IsNullOrWhiteSpace(o.Value) ? o.Label : o.Value)
                    .ToHashSet(StringComparer.Ordinal);
                if (values.Any(v => !allowed.Contains(v)))
                    return $"\"{field.Label}\" contains an invalid choice.";
                if (type != FormFieldType.Checkbox && values.Count > 1)
                    return $"\"{field.Label}\" accepts a single choice.";
                break;
            }
            case FormFieldType.Rating:
            case FormFieldType.Number:
            {
                if (!double.TryParse(values[0], out var number))
                    return $"\"{field.Label}\" must be a number.";
                var min = field.MinValue ?? (type == FormFieldType.Rating ? 1 : (int?)null);
                var max = field.MaxValue ?? (type == FormFieldType.Rating ? 5 : (int?)null);
                if (min is int lo && number < lo) return $"\"{field.Label}\" must be at least {lo}.";
                if (max is int hi && number > hi) return $"\"{field.Label}\" must be at most {hi}.";
                break;
            }
            case FormFieldType.Email:
                if (!values[0].Contains('@') || values[0].Length > 200)
                    return $"\"{field.Label}\" must be a valid email address.";
                break;
            case FormFieldType.YesNo:
                if (values[0] is not ("yes" or "no"))
                    return $"\"{field.Label}\" must be yes or no.";
                break;
            case FormFieldType.Text:
            case FormFieldType.TextArea:
            {
                if (field.MaxValue is int maxLen && values[0].Length > maxLen)
                    return $"\"{field.Label}\" must be at most {maxLen} characters.";
                break;
            }
        }
        return null;
    }

    private static FormFieldReportDto BuildFieldReport(FormField field, List<string?> rawValues)
    {
        var type = (FormFieldType)field.FieldType;
        var flat = rawValues.SelectMany(SplitStoredValue).ToList();

        double? average = null;
        var optionCounts = new List<FormOptionCountDto>();
        var textSamples = new List<string>();

        switch (type)
        {
            case FormFieldType.Select:
            case FormFieldType.Radio:
            case FormFieldType.Checkbox:
            {
                // Count every configured option (even zero-answer ones) so charts show the full scale.
                var counts = flat.GroupBy(v => v, StringComparer.Ordinal)
                    .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
                var known = new HashSet<string>(StringComparer.Ordinal);
                foreach (var option in field.FormFieldOptions.OrderBy(o => o.SortOrder).ThenBy(o => o.FormFieldOptionID))
                {
                    var value = string.IsNullOrWhiteSpace(option.Value) ? option.Label : option.Value;
                    known.Add(value);
                    optionCounts.Add(BuildCount(option.Label, counts.TryGetValue(value, out var c) ? c : 0, flat.Count));
                }
                foreach (var (value, count) in counts.Where(kv => !known.Contains(kv.Key)))
                    optionCounts.Add(BuildCount(value, count, flat.Count));
                break;
            }
            case FormFieldType.Rating:
            {
                var numbers = flat.Select(v => double.TryParse(v, out var n) ? n : (double?)null)
                    .Where(n => n is not null).Select(n => n!.Value).ToList();
                if (numbers.Count > 0) average = Math.Round(numbers.Average(), 2);
                var min = field.MinValue ?? 1;
                var max = field.MaxValue ?? 5;
                for (int score = min; score <= max; score++)
                {
                    var count = numbers.Count(n => (int)n == score);
                    optionCounts.Add(BuildCount(score.ToString(), count, numbers.Count));
                }
                break;
            }
            case FormFieldType.Number:
            {
                var numbers = flat.Select(v => double.TryParse(v, out var n) ? n : (double?)null)
                    .Where(n => n is not null).Select(n => n!.Value).ToList();
                if (numbers.Count > 0) average = Math.Round(numbers.Average(), 2);
                break;
            }
            case FormFieldType.YesNo:
            {
                optionCounts.Add(BuildCount("Yes", flat.Count(v => v == "yes"), flat.Count));
                optionCounts.Add(BuildCount("No", flat.Count(v => v == "no"), flat.Count));
                break;
            }
            default:
                textSamples = flat.Take(TextSampleCount).ToList();
                break;
        }

        return new FormFieldReportDto
        {
            FormFieldID = field.FormFieldID,
            Label = field.Label,
            FieldType = type,
            AnswerCount = rawValues.Count,
            Average = average,
            OptionCounts = optionCounts,
            LatestTextAnswers = textSamples,
        };
    }

    private static FormOptionCountDto BuildCount(string label, int count, int total) => new()
    {
        Label = label,
        Count = count,
        Percent = total == 0 ? 0 : Math.Round(count * 100.0 / total, 1),
    };

    /// <summary>Checkbox answers may be stored as a JSON array — flatten for aggregation/display.</summary>
    private static IEnumerable<string> SplitStoredValue(string? stored)
    {
        if (string.IsNullOrEmpty(stored)) yield break;
        if (stored.StartsWith('[') && stored.EndsWith(']'))
        {
            List<string>? parsed = null;
            try { parsed = JsonSerializer.Deserialize<List<string>>(stored); }
            catch (JsonException) { /* legacy/plain value that just looks like JSON */ }
            if (parsed is not null)
            {
                foreach (var item in parsed) yield return item;
                yield break;
            }
        }
        yield return stored;
    }

    private static string DisplayValue(string? stored) =>
        string.Join(", ", SplitStoredValue(stored));

    private static string NormalizeSlug(string? slug, string title)
    {
        var source = string.IsNullOrWhiteSpace(slug) ? title : slug;
        var sb = new StringBuilder(source.Length);
        foreach (var c in source.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c)) sb.Append(c);
            else if (c is ' ' or '-' or '_' && sb.Length > 0 && sb[^1] != '-') sb.Append('-');
        }
        return sb.ToString().Trim('-');
    }
}
