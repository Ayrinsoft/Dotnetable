using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.DTOs;

/// <summary>A dynamic form/survey definition rendered by the public site (Web/React).</summary>
public sealed class FormDto
{
    public int FormID { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public FormType FormType { get; init; }
    public string? SubmitButtonText { get; init; }
    public string? SuccessMessage { get; init; }
    public bool RequireLogin { get; init; }
    public bool AllowMultipleSubmissions { get; init; }
    public bool ShowResults { get; init; }
    /// <summary>False when the form is outside its Start/End window — render it read-only with a notice.</summary>
    public bool IsOpen { get; init; } = true;
    public List<FormFieldDto> Fields { get; init; } = new();
}

public sealed class FormFieldDto
{
    public int FormFieldID { get; init; }
    public string Label { get; init; } = string.Empty;
    public FormFieldType FieldType { get; init; }
    public string? Placeholder { get; init; }
    public string? HelpText { get; init; }
    public bool IsRequired { get; init; }
    public int? MinValue { get; init; }
    public int? MaxValue { get; init; }
    public List<FormFieldOptionDto> Options { get; init; } = new();
}

public sealed class FormFieldOptionDto
{
    public int FormFieldOptionID { get; init; }
    public string Label { get; init; } = string.Empty;
    /// <summary>Stored answer value (falls back to the label when the option has no explicit value).</summary>
    public string Value { get; init; } = string.Empty;
}

/// <summary>One submitted answer: the field plus one value (or several for checkbox fields).</summary>
public sealed class FormAnswerInput
{
    public int FormFieldID { get; set; }
    public List<string> Values { get; set; } = new();
}

public sealed class FormSubmissionRequest
{
    public List<FormAnswerInput> Answers { get; set; } = new();
}

public sealed class FormSubmissionResult
{
    public bool Ok { get; init; }
    public string? Message { get; init; }

    public static FormSubmissionResult Success(string? message = null) => new() { Ok = true, Message = message };
    public static FormSubmissionResult Fail(string message) => new() { Ok = false, Message = message };
}

// ── Reporting ───────────────────────────────────────────────────────

/// <summary>Aggregate report for a form: response totals plus a per-field breakdown.</summary>
public sealed class FormReportDto
{
    public int FormID { get; init; }
    public string Title { get; init; } = string.Empty;
    public FormType FormType { get; init; }
    public int ResponseCount { get; init; }
    public DateTime? FirstResponseAt { get; init; }
    public DateTime? LastResponseAt { get; init; }
    public List<FormFieldReportDto> Fields { get; init; } = new();
}

public sealed class FormFieldReportDto
{
    public int FormFieldID { get; init; }
    public string Label { get; init; } = string.Empty;
    public FormFieldType FieldType { get; init; }
    public int AnswerCount { get; init; }
    /// <summary>Mean of numeric answers — filled for Rating and Number fields.</summary>
    public double? Average { get; init; }
    /// <summary>Answer distribution for choice fields (Select/Radio/Checkbox/YesNo) and ratings.</summary>
    public List<FormOptionCountDto> OptionCounts { get; init; } = new();
    /// <summary>Most recent free-text answers (capped) for Text/TextArea fields.</summary>
    public List<string> LatestTextAnswers { get; init; } = new();
}

public sealed class FormOptionCountDto
{
    public string Label { get; init; } = string.Empty;
    public int Count { get; init; }
    public double Percent { get; init; }
}

/// <summary>One response row in the admin responses grid.</summary>
public sealed class FormResponseListItemDto
{
    public int FormResponseID { get; init; }
    public DateTime SubmittedAt { get; init; }
    public string? ClientDisplayName { get; init; }
    public string SenderIPAddress { get; init; } = string.Empty;
    /// <summary>Answers keyed by FormFieldID (multi-values already joined for display).</summary>
    public Dictionary<int, string> Answers { get; init; } = new();
}

/// <summary>Public aggregate results shown to survey respondents when <c>Form.ShowResults</c> is on.</summary>
public sealed class FormPublicResultsDto
{
    public int FormID { get; init; }
    public string Title { get; init; } = string.Empty;
    public int ResponseCount { get; init; }
    public List<FormFieldReportDto> Fields { get; init; } = new();
}
