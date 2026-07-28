using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Dynamic form / survey engine. Admin builds a form out of typed fields (with options for
/// choice types); the public site renders it (standalone page or <c>[form:ID]</c> shortcode),
/// collects responses, and the admin gets aggregate reports plus an Excel export.
/// </summary>
public interface IFormService
{
    // ── Admin: forms ────────────────────────────────────────────────
    Task<List<Form>> GetFormsAsync(int? websiteId, CancellationToken ct = default);
    /// <summary>Paged / sorted / filtered forms for the admin grid (includes field &amp; response counts).</summary>
    Task<PagedResult<Form>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);
    Task<Form?> GetFormAsync(int formId, CancellationToken ct = default);
    Task<Form> CreateFormAsync(Form form, CancellationToken ct = default);
    Task UpdateFormAsync(Form form, CancellationToken ct = default);
    Task DeleteFormAsync(int formId, CancellationToken ct = default);

    // ── Admin: fields ───────────────────────────────────────────────
    Task<List<FormField>> GetFieldsAsync(int formId, CancellationToken ct = default);
    /// <summary>Creates or updates a field; <paramref name="options"/> fully replaces the option list.</summary>
    Task<FormField> SaveFieldAsync(FormField field, List<FormFieldOption> options, CancellationToken ct = default);
    Task DeleteFieldAsync(int fieldId, CancellationToken ct = default);
    Task ReorderFieldsAsync(int formId, List<int> orderedFieldIds, CancellationToken ct = default);

    // ── Admin: reporting ────────────────────────────────────────────
    Task<FormReportDto?> GetReportAsync(int formId, CancellationToken ct = default);
    Task<PagedResult<FormResponseListItemDto>> GetResponsesAsync(int formId, int page, int pageSize, CancellationToken ct = default);
    Task DeleteResponseAsync(int responseId, CancellationToken ct = default);
    /// <summary>All responses as .xlsx — one column per field, one row per response.</summary>
    Task<byte[]> ExportResponsesExcelAsync(int formId, CancellationToken ct = default);

    // ── Public (API-facing) ─────────────────────────────────────────
    Task<FormDto?> GetPublicFormAsync(int websiteId, string slug, CancellationToken ct = default);
    Task<FormDto?> GetPublicFormByIdAsync(int websiteId, int formId, CancellationToken ct = default);
    /// <summary>Validates and stores a submission. Enforces active/date-window/required/option checks
    /// and the single-submission rule for signed-in clients.</summary>
    Task<FormSubmissionResult> SubmitAsync(int websiteId, int formId, int? websiteClientId, string senderIp, FormSubmissionRequest request, CancellationToken ct = default);
    /// <summary>Aggregate results for surveys with <c>ShowResults</c> enabled; null otherwise.</summary>
    Task<FormPublicResultsDto?> GetPublicResultsAsync(int websiteId, int formId, CancellationToken ct = default);
}
