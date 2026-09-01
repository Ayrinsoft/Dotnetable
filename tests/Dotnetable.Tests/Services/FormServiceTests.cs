using System.Text;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Dotnetable.Tests.Services;

public class FormServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly FormService _service;
    private readonly Website _website;

    public FormServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        // Services open a context per call now, so they get a factory over the same options;
        // the fixture keeps its own _context for seeding and asserting.
        var factory = new TestDbContextFactory(opts);
        _service = new FormService(factory, new Mock<IAdminNotificationService>().Object);

        _website = NewWebsite("Test", "test.com");
        _context.Websites.Add(_website);
        _context.SaveChanges();
    }

    private static Website NewWebsite(string trade, string address) => new()
    {
        TradeName = trade, BrandName = trade, WebsiteAddress = address,
        AuthCode = Guid.NewGuid(), Active = true, Manager = "Mgr", Mobile = "123",
        Email = $"admin@{address}", RegisterDate = DateOnly.FromDateTime(DateTime.Today),
        DefaultLanguageCode = "en", DefaultCurrencyCode = "USD",
    };

    private Form NewForm(string title = "Feedback", byte type = (byte)FormType.Form) => new()
    {
        WebsiteID = _website.WebsiteID,
        Title = title,
        Slug = string.Empty,
        FormType = type,
        AllowMultipleSubmissions = true,
        IsActive = true,
    };

    private async Task<(Form Form, FormField Text, FormField Radio, FormField Rating)> SeedFormWithFieldsAsync(
        bool showResults = false, bool requireLogin = false, bool allowMultiple = true)
    {
        var form = NewForm();
        form.ShowResults = showResults;
        form.RequireLogin = requireLogin;
        form.AllowMultipleSubmissions = allowMultiple;
        await _service.CreateFormAsync(form);

        var text = await _service.SaveFieldAsync(new FormField
        {
            FormID = form.FormID, Label = "Your comment",
            FieldType = (byte)FormFieldType.Text, IsRequired = true, IsActive = true,
        }, new List<FormFieldOption>());

        var radio = await _service.SaveFieldAsync(new FormField
        {
            FormID = form.FormID, Label = "Favorite color",
            FieldType = (byte)FormFieldType.Radio, IsActive = true,
        }, new List<FormFieldOption>
        {
            new() { Label = "Red" },
            new() { Label = "Blue", Value = "blu" },
        });

        var rating = await _service.SaveFieldAsync(new FormField
        {
            FormID = form.FormID, Label = "Score",
            FieldType = (byte)FormFieldType.Rating, MinValue = 1, MaxValue = 5, IsActive = true,
        }, new List<FormFieldOption>());

        return (form, text, radio, rating);
    }

    private static FormSubmissionRequest Answers(params (int FieldId, string[] Values)[] answers) => new()
    {
        Answers = answers.Select(a => new FormAnswerInput
        {
            FormFieldID = a.FieldId,
            Values = a.Values.ToList(),
        }).ToList(),
    };

    // ── Forms CRUD ─────────────────────────────────────────────────

    [Fact]
    public async Task CreateFormAsync_GeneratesSlugFromTitle_WhenSlugBlank()
    {
        var form = await _service.CreateFormAsync(NewForm("Customer Survey 2026"));

        form.FormID.Should().BeGreaterThan(0);
        form.Slug.Should().Be("customer-survey-2026");
    }

    [Fact]
    public async Task CreateFormAsync_SetsCreatedAtUtc()
    {
        var before = DateTime.UtcNow.AddSeconds(-2);

        var form = await _service.CreateFormAsync(NewForm("Timed Form"));

        form.CreatedAt.Should().BeOnOrAfter(before);
        form.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        (await _context.Forms.FindAsync(form.FormID))!.CreatedAt.Should().Be(form.CreatedAt);
    }

    [Fact]
    public async Task CreateFormAsync_NormalizesExplicitSlug()
    {
        var form = NewForm();
        form.Slug = "  My Fancy_Slug  ";

        (await _service.CreateFormAsync(form)).Slug.Should().Be("my-fancy-slug");
    }

    [Fact]
    public async Task GetFormsAsync_WebsiteFilter_ReturnsOnlyMatchingSite()
    {
        var other = NewWebsite("Other", "other.com");
        _context.Websites.Add(other);
        await _context.SaveChangesAsync();

        await _service.CreateFormAsync(NewForm("Mine"));
        await _service.CreateFormAsync(new Form
        {
            WebsiteID = other.WebsiteID, Title = "Theirs", Slug = "theirs", IsActive = true,
        });

        var result = await _service.GetFormsAsync(_website.WebsiteID);

        result.Should().ContainSingle(f => f.Title == "Mine");
    }

    [Fact]
    public async Task GetFormsAsync_NullWebsite_ReturnsAll()
    {
        await _service.CreateFormAsync(NewForm("A"));
        await _service.CreateFormAsync(NewForm("B"));

        (await _service.GetFormsAsync(null)).Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateFormAsync_PersistsChanges()
    {
        var form = await _service.CreateFormAsync(NewForm());
        form.Title = "Updated";
        form.Slug = "updated";

        await _service.UpdateFormAsync(form);

        (await _context.Forms.FindAsync(form.FormID))!.Title.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteFormAsync_RemovesFieldsOptionsAndResponses()
    {
        var (form, text, radio, _) = await SeedFormWithFieldsAsync();
        await _service.SubmitAsync(_website.WebsiteID, form.FormID, null, "1.2.3.4",
            Answers((text.FormFieldID, ["hello"]), (radio.FormFieldID, ["Red"])));

        await _service.DeleteFormAsync(form.FormID);

        _context.Forms.Should().BeEmpty();
        _context.FormFields.Should().BeEmpty();
        _context.FormFieldOptions.Should().BeEmpty();
        _context.FormResponses.Should().BeEmpty();
        _context.FormResponseValues.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteFormAsync_MissingId_DoesNotThrow()
    {
        await _service.Invoking(s => s.DeleteFormAsync(999)).Should().NotThrowAsync();
    }

    // ── Fields ─────────────────────────────────────────────────────

    [Fact]
    public async Task SaveFieldAsync_NewFields_GetIncreasingSortOrder()
    {
        var form = await _service.CreateFormAsync(NewForm());

        var first = await _service.SaveFieldAsync(new FormField { FormID = form.FormID, Label = "A", IsActive = true }, new());
        var second = await _service.SaveFieldAsync(new FormField { FormID = form.FormID, Label = "B", IsActive = true }, new());

        second.SortOrder.Should().BeGreaterThan(first.SortOrder);
    }

    [Fact]
    public async Task SaveFieldAsync_Update_ReplacesOptionList()
    {
        var (_, _, radio, _) = await SeedFormWithFieldsAsync();

        await _service.SaveFieldAsync(radio, new List<FormFieldOption>
        {
            new() { Label = "Green" },
        });

        var options = _context.FormFieldOptions.Where(o => o.FormFieldID == radio.FormFieldID).ToList();
        options.Should().ContainSingle(o => o.Label == "Green");
    }

    [Fact]
    public async Task DeleteFieldAsync_RemovesOptionsAndAnswers()
    {
        var (form, text, radio, _) = await SeedFormWithFieldsAsync();
        await _service.SubmitAsync(_website.WebsiteID, form.FormID, null, "1.1.1.1",
            Answers((text.FormFieldID, ["x"]), (radio.FormFieldID, ["Red"])));

        await _service.DeleteFieldAsync(radio.FormFieldID);

        _context.FormFields.Should().NotContain(f => f.FormFieldID == radio.FormFieldID);
        _context.FormFieldOptions.Should().BeEmpty();
        _context.FormResponseValues.Should().OnlyContain(v => v.FormFieldID == text.FormFieldID);
    }

    [Fact]
    public async Task ReorderFieldsAsync_AppliesGivenOrder()
    {
        var (form, text, radio, rating) = await SeedFormWithFieldsAsync();

        await _service.ReorderFieldsAsync(form.FormID,
            [rating.FormFieldID, text.FormFieldID, radio.FormFieldID]);

        var ordered = (await _service.GetFieldsAsync(form.FormID)).Select(f => f.FormFieldID);
        ordered.Should().ContainInOrder(rating.FormFieldID, text.FormFieldID, radio.FormFieldID);
    }

    // ── Public read ────────────────────────────────────────────────

    [Fact]
    public async Task GetPublicFormAsync_ReturnsProjectionWithOptionValueFallback()
    {
        var (form, _, _, _) = await SeedFormWithFieldsAsync();

        var dto = await _service.GetPublicFormAsync(_website.WebsiteID, form.Slug);

        dto.Should().NotBeNull();
        dto!.IsOpen.Should().BeTrue();
        dto.Fields.Should().HaveCount(3);
        var radioOptions = dto.Fields.Single(f => f.FieldType == FormFieldType.Radio).Options;
        radioOptions.Select(o => o.Value).Should().ContainInOrder("Red", "blu"); // value falls back to label
    }

    [Fact]
    public async Task GetPublicFormAsync_WrongWebsite_ReturnsNull()
    {
        var (form, _, _, _) = await SeedFormWithFieldsAsync();

        (await _service.GetPublicFormAsync(_website.WebsiteID + 1, form.Slug)).Should().BeNull();
    }

    [Fact]
    public async Task GetPublicFormByIdAsync_InactiveForm_ReturnsNull()
    {
        var form = await _service.CreateFormAsync(NewForm());
        form.IsActive = false;
        await _service.UpdateFormAsync(form);

        (await _service.GetPublicFormByIdAsync(_website.WebsiteID, form.FormID)).Should().BeNull();
    }

    [Fact]
    public async Task GetPublicFormAsync_OutsideDateWindow_ReportsClosed()
    {
        var form = NewForm();
        form.EndAt = DateTime.UtcNow.AddDays(-1);
        await _service.CreateFormAsync(form);
        await _service.SaveFieldAsync(new FormField { FormID = form.FormID, Label = "Q", IsActive = true }, new());

        var dto = await _service.GetPublicFormAsync(_website.WebsiteID, form.Slug);

        dto!.IsOpen.Should().BeFalse();
    }

    // ── Submit ─────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitAsync_ValidAnswers_StoresResponse()
    {
        var (form, text, radio, rating) = await SeedFormWithFieldsAsync();

        var result = await _service.SubmitAsync(_website.WebsiteID, form.FormID, null, "9.9.9.9",
            Answers((text.FormFieldID, ["Great product"]), (radio.FormFieldID, ["blu"]), (rating.FormFieldID, ["4"])));

        result.Ok.Should().BeTrue();
        var response = _context.FormResponses.Include(r => r.FormResponseValues).Single();
        response.SenderIPAddress.Should().Be("9.9.9.9");
        response.FormResponseValues.Should().HaveCount(3);
    }

    [Fact]
    public async Task SubmitAsync_MissingRequiredField_Fails()
    {
        var (form, _, radio, _) = await SeedFormWithFieldsAsync();

        var result = await _service.SubmitAsync(_website.WebsiteID, form.FormID, null, "1.1.1.1",
            Answers((radio.FormFieldID, ["Red"])));

        result.Ok.Should().BeFalse();
        result.Message.Should().Contain("Your comment");
        _context.FormResponses.Should().BeEmpty();
    }

    [Fact]
    public async Task SubmitAsync_InvalidChoice_Fails()
    {
        var (form, text, radio, _) = await SeedFormWithFieldsAsync();

        var result = await _service.SubmitAsync(_website.WebsiteID, form.FormID, null, "1.1.1.1",
            Answers((text.FormFieldID, ["ok"]), (radio.FormFieldID, ["Purple"])));

        result.Ok.Should().BeFalse();
        result.Message.Should().Contain("invalid choice");
    }

    [Fact]
    public async Task SubmitAsync_MultipleValuesForSingleChoice_Fails()
    {
        var (form, text, radio, _) = await SeedFormWithFieldsAsync();

        var result = await _service.SubmitAsync(_website.WebsiteID, form.FormID, null, "1.1.1.1",
            Answers((text.FormFieldID, ["ok"]), (radio.FormFieldID, ["Red", "blu"])));

        result.Ok.Should().BeFalse();
    }

    [Fact]
    public async Task SubmitAsync_RatingOutOfBounds_Fails()
    {
        var (form, text, _, rating) = await SeedFormWithFieldsAsync();

        var result = await _service.SubmitAsync(_website.WebsiteID, form.FormID, null, "1.1.1.1",
            Answers((text.FormFieldID, ["ok"]), (rating.FormFieldID, ["9"])));

        result.Ok.Should().BeFalse();
        result.Message.Should().Contain("at most 5");
    }

    [Fact]
    public async Task SubmitAsync_CheckboxMultiValue_StoredAsJsonArray()
    {
        var form = await _service.CreateFormAsync(NewForm());
        var checkbox = await _service.SaveFieldAsync(new FormField
        {
            FormID = form.FormID, Label = "Toppings",
            FieldType = (byte)FormFieldType.Checkbox, IsActive = true,
        }, new List<FormFieldOption> { new() { Label = "Cheese" }, new() { Label = "Olives" } });

        var result = await _service.SubmitAsync(_website.WebsiteID, form.FormID, null, "1.1.1.1",
            Answers((checkbox.FormFieldID, ["Cheese", "Olives"])));

        result.Ok.Should().BeTrue();
        _context.FormResponseValues.Single().Value.Should().Be("[\"Cheese\",\"Olives\"]");
    }

    [Fact]
    public async Task SubmitAsync_RequireLogin_AnonymousFails()
    {
        var (form, text, _, _) = await SeedFormWithFieldsAsync(requireLogin: true);

        var result = await _service.SubmitAsync(_website.WebsiteID, form.FormID, null, "1.1.1.1",
            Answers((text.FormFieldID, ["hi"])));

        result.Ok.Should().BeFalse();
        result.Message.Should().Contain("sign in");
    }

    [Fact]
    public async Task SubmitAsync_SingleSubmissionRule_BlocksSecondSubmitBySameClient()
    {
        var (form, text, _, _) = await SeedFormWithFieldsAsync(allowMultiple: false);
        var client = new WebsiteClient
        {
            WebsiteID = _website.WebsiteID, Active = true, HashKey = Guid.NewGuid(),
            RegisterDate = DateOnly.FromDateTime(DateTime.Today),
        };
        _context.WebsiteClients.Add(client);
        await _context.SaveChangesAsync();

        var first = await _service.SubmitAsync(_website.WebsiteID, form.FormID, client.WebsiteClientID, "1.1.1.1",
            Answers((text.FormFieldID, ["one"])));
        var second = await _service.SubmitAsync(_website.WebsiteID, form.FormID, client.WebsiteClientID, "1.1.1.1",
            Answers((text.FormFieldID, ["two"])));

        first.Ok.Should().BeTrue();
        second.Ok.Should().BeFalse();
        second.Message.Should().Contain("already");
    }

    [Fact]
    public async Task SubmitAsync_ClosedForm_Fails()
    {
        var form = NewForm();
        form.EndAt = DateTime.UtcNow.AddDays(-1);
        await _service.CreateFormAsync(form);
        var field = await _service.SaveFieldAsync(
            new FormField { FormID = form.FormID, Label = "Q", IsActive = true }, new());

        var result = await _service.SubmitAsync(_website.WebsiteID, form.FormID, null, "1.1.1.1",
            Answers((field.FormFieldID, ["x"])));

        result.Ok.Should().BeFalse();
    }

    [Fact]
    public async Task SubmitAsync_UnknownForm_Fails()
    {
        var result = await _service.SubmitAsync(_website.WebsiteID, 999, null, "1.1.1.1", Answers());

        result.Ok.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task SubmitAsync_EmptySubmission_Fails()
    {
        var form = await _service.CreateFormAsync(NewForm());
        await _service.SaveFieldAsync(new FormField { FormID = form.FormID, Label = "Optional", IsActive = true }, new());

        var result = await _service.SubmitAsync(_website.WebsiteID, form.FormID, null, "1.1.1.1", Answers());

        result.Ok.Should().BeFalse();
        _context.FormResponses.Should().BeEmpty();
    }

    [Fact]
    public async Task SubmitAsync_ReturnsConfiguredSuccessMessage()
    {
        var form = NewForm();
        form.SuccessMessage = "Thanks a lot!";
        await _service.CreateFormAsync(form);
        var field = await _service.SaveFieldAsync(
            new FormField { FormID = form.FormID, Label = "Q", IsActive = true }, new());

        var result = await _service.SubmitAsync(_website.WebsiteID, form.FormID, null, "1.1.1.1",
            Answers((field.FormFieldID, ["hello"])));

        result.Ok.Should().BeTrue();
        result.Message.Should().Be("Thanks a lot!");
    }

    // ── Reporting ──────────────────────────────────────────────────

    [Fact]
    public async Task GetReportAsync_AggregatesOptionCountsRatingAverageAndTextSamples()
    {
        var (form, text, radio, rating) = await SeedFormWithFieldsAsync();
        await _service.SubmitAsync(_website.WebsiteID, form.FormID, null, "1.1.1.1",
            Answers((text.FormFieldID, ["first"]), (radio.FormFieldID, ["Red"]), (rating.FormFieldID, ["2"])));
        await _service.SubmitAsync(_website.WebsiteID, form.FormID, null, "2.2.2.2",
            Answers((text.FormFieldID, ["second"]), (radio.FormFieldID, ["Red"]), (rating.FormFieldID, ["4"])));

        var report = await _service.GetReportAsync(form.FormID);

        report.Should().NotBeNull();
        report!.ResponseCount.Should().Be(2);

        var radioReport = report.Fields.Single(f => f.FormFieldID == radio.FormFieldID);
        radioReport.OptionCounts.Single(o => o.Label == "Red").Count.Should().Be(2);
        radioReport.OptionCounts.Single(o => o.Label == "Red").Percent.Should().Be(100);
        radioReport.OptionCounts.Single(o => o.Label == "Blue").Count.Should().Be(0);

        report.Fields.Single(f => f.FormFieldID == rating.FormFieldID).Average.Should().Be(3);
        report.Fields.Single(f => f.FormFieldID == text.FormFieldID)
            .LatestTextAnswers.Should().Contain(["first", "second"]);
    }

    [Fact]
    public async Task GetReportAsync_MissingForm_ReturnsNull()
    {
        (await _service.GetReportAsync(999)).Should().BeNull();
    }

    [Fact]
    public async Task GetResponsesAsync_PagesNewestFirst()
    {
        var (form, text, _, _) = await SeedFormWithFieldsAsync();
        for (int i = 1; i <= 5; i++)
            await _service.SubmitAsync(_website.WebsiteID, form.FormID, null, $"1.1.1.{i}",
                Answers((text.FormFieldID, [$"answer {i}"])));

        var page = await _service.GetResponsesAsync(form.FormID, page: 1, pageSize: 2);

        page.TotalCount.Should().Be(5);
        page.Items.Should().HaveCount(2);
        page.Items[0].FormResponseID.Should().BeGreaterThan(page.Items[1].FormResponseID);
        page.Items[0].Answers[text.FormFieldID].Should().Be("answer 5");
    }

    [Fact]
    public async Task DeleteResponseAsync_RemovesResponseAndValues()
    {
        var (form, text, _, _) = await SeedFormWithFieldsAsync();
        await _service.SubmitAsync(_website.WebsiteID, form.FormID, null, "1.1.1.1",
            Answers((text.FormFieldID, ["bye"])));
        var responseId = _context.FormResponses.Single().FormResponseID;

        await _service.DeleteResponseAsync(responseId);

        _context.FormResponses.Should().BeEmpty();
        _context.FormResponseValues.Should().BeEmpty();
    }

    [Fact]
    public async Task ExportResponsesExcelAsync_ProducesHeaderAndRows()
    {
        var (form, text, radio, _) = await SeedFormWithFieldsAsync();
        await _service.SubmitAsync(_website.WebsiteID, form.FormID, null, "1.1.1.1",
            Answers((text.FormFieldID, ["hello, world"]), (radio.FormFieldID, ["Red"])));

        var bytes = await _service.ExportResponsesExcelAsync(form.FormID);

        bytes.Length.Should().BeGreaterThan(4);
        // xlsx is a ZIP package
        bytes[0].Should().Be((byte)'P');
        bytes[1].Should().Be((byte)'K');

        using var package = new OfficeOpenXml.ExcelPackage(new MemoryStream(bytes));
        var sheet = package.Workbook.Worksheets.First();
        sheet.Cells[1, 1].Text.Should().Be("ResponseID");
        sheet.Cells[1, 2].Text.Should().Be("SubmittedAtUtc");
        sheet.Cells[1, 5].Text.Should().Be("Your comment");
        sheet.Cells[1, 6].Text.Should().Be("Favorite color");
        sheet.Cells[2, 4].Text.Should().Be("1.1.1.1");
        sheet.Cells[2, 5].Text.Should().Be("hello, world");
        sheet.Cells[2, 6].Text.Should().Be("Red");
    }

    [Fact]
    public async Task ExportResponsesExcelAsync_MissingForm_Throws()
    {
        await _service.Invoking(s => s.ExportResponsesExcelAsync(999))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    // ── Public results ─────────────────────────────────────────────

    [Fact]
    public async Task GetPublicResultsAsync_ShowResultsDisabled_ReturnsNull()
    {
        var (form, text, _, _) = await SeedFormWithFieldsAsync(showResults: false);
        await _service.SubmitAsync(_website.WebsiteID, form.FormID, null, "1.1.1.1",
            Answers((text.FormFieldID, ["x"])));

        (await _service.GetPublicResultsAsync(_website.WebsiteID, form.FormID)).Should().BeNull();
    }

    [Fact]
    public async Task GetPublicResultsAsync_ExcludesFreeTextFields()
    {
        var (form, text, radio, rating) = await SeedFormWithFieldsAsync(showResults: true);
        await _service.SubmitAsync(_website.WebsiteID, form.FormID, null, "1.1.1.1",
            Answers((text.FormFieldID, ["private text"]), (radio.FormFieldID, ["Red"]), (rating.FormFieldID, ["5"])));

        var results = await _service.GetPublicResultsAsync(_website.WebsiteID, form.FormID);

        results.Should().NotBeNull();
        results!.ResponseCount.Should().Be(1);
        results.Fields.Should().NotContain(f => f.FormFieldID == text.FormFieldID);
        results.Fields.Should().Contain(f => f.FormFieldID == radio.FormFieldID);
        results.Fields.Should().OnlyContain(f => f.LatestTextAnswers.Count == 0);
    }

    public void Dispose() => _context.Dispose();
}
