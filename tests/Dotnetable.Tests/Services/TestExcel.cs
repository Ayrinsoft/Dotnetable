using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Excel;

namespace Dotnetable.Tests.Services;

internal static class TestExcel
{
    public static IExcelExporterRegistry Registry { get; } = new ExcelExporterRegistry(
    [
        new ClosedXmlExcelExporter(),
        new MiniExcelExcelExporter(),
    ]);
}
