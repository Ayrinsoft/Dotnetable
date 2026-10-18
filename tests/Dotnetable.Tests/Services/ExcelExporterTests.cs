using Dotnetable.Infrastructure.Excel;
using FluentAssertions;
using Xunit;

namespace Dotnetable.Tests.Services;

public class ExcelExporterTests
{
    [Fact]
    public void Select_UsesClosedXmlBelowTheThreshold_AndMiniExcelAtIt()
    {
        var registry = TestExcel.Registry;
        registry.Select(registry.HeavyRowThreshold - 1).Key.Should().Be("closedxml");
        registry.Select(registry.HeavyRowThreshold).Key.Should().Be("miniexcel");
    }

    [Fact]
    public void ClosedXml_RoundTripsHeaderAndValues()
    {
        var excel = new ClosedXmlExcelExporter();
        var bytes = excel.Write("Countries", ["CountryCode", "Title"], [["IR", "Iran"]]);
        var rows = excel.Read(new MemoryStream(bytes));
        rows.Should().HaveCount(2);
        rows[0].Should().Equal("CountryCode", "Title");
        rows[1].Should().Equal("IR", "Iran");
    }

    [Fact]
    public void MiniExcel_RoundTrips_AndClosedXmlCanReadIt()
    {
        var heavy = new MiniExcelExcelExporter();
        var bytes = heavy.Write("Payroll", ["Code", "Net"], [["E1", "10"], ["E2", "20"]]);
        var fromMini = heavy.Read(new MemoryStream(bytes));
        fromMini.Should().HaveCount(3);
        fromMini[0].Should().Equal("Code", "Net");
        fromMini[1].Should().Equal("E1", "10");
        fromMini[2].Should().Equal("E2", "20");

        var fromClosed = new ClosedXmlExcelExporter().Read(new MemoryStream(bytes));
        fromClosed.Should().HaveCount(3);
        fromClosed[0].Should().Equal("Code", "Net");
        fromClosed[1].Should().Equal("E1", "10");
        fromClosed[2].Should().Equal("E2", "20");
    }

    [Fact]
    public void Registry_HeavySheet_IsReadableByTheStandardReader()
    {
        var rows = Enumerable.Range(1, TestExcel.Registry.HeavyRowThreshold)
            .Select(i => (IReadOnlyList<object?>)new object?[] { i.ToString(), "ok" });
        var bytes = TestExcel.Registry.Write("Bulk", ["N", "Status"], rows);
        var read = TestExcel.Registry.Read(new MemoryStream(bytes));
        read.Should().HaveCount(TestExcel.Registry.HeavyRowThreshold + 1);
        read[0].Should().Equal("N", "Status");
        read[^1].Should().Equal(TestExcel.Registry.HeavyRowThreshold.ToString(), "ok");
    }
}
