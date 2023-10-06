using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Dotnetable.Admin.Pages;

public partial class Index
{
    [Inject] private IStringLocalizer<Shared.Resources.Resource> _loc { get; set; }

    private const int TotalActiveUsers = 18765;
    private const double IncreaseDecreaseActiveUsers = 2.6;
    private const int TotalInstalls = 4876;
    private const double IncreaseDecreaseInstalls = 0.2;
    private const int TotalDownloads = 678;
    private const double IncreaseDecreaseDownloads = -0.1;

    private readonly List<int> _lastTenDaysActiveUsers = new() { 20, 41, 63, 23, 38, 25, 50, 46, 11, 26 };
    private readonly List<int> _lastTenDaysDownloads = new() { 20, 41, 63, 23, 38, 25, 50, 46, 11, 26 };
    private readonly List<int> _lastTenDaysInstalls = new() { 20, 41, 63, 23, 38, 25, 50, 46, 11, 26 };
}