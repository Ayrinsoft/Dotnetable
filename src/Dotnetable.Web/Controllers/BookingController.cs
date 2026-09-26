using Dotnetable.Application.DTOs;
using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

/// <summary>Customer appointment calendar. Payment uses the existing order checkout page.</summary>
public class BookingController : Controller
{
    private readonly ApiClient _api;

    public BookingController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var catalog = await _api.GetBookingCatalogAsync(ct) ?? new BookingCatalogDto();
        return View(catalog);
    }

    public async Task<IActionResult> Book(int offeringId, int? year, int? month, string? date, CancellationToken ct)
    {
        var catalog = await _api.GetBookingCatalogAsync(ct);
        var offering = catalog?.Resources.SelectMany(r => r.Offerings).FirstOrDefault(o => o.OfferingId == offeringId);
        if (catalog is null || offering is null) return NotFound();

        var today = DateTime.Today;
        var y = year is > 2000 ? year.Value : today.Year;
        var m = month is >= 1 and <= 12 ? month.Value : today.Month;
        var days = await _api.GetBookingDaysAsync(offeringId, y, m, ct);
        IReadOnlyList<BookingSlotDto> slots = string.IsNullOrWhiteSpace(date)
            ? Array.Empty<BookingSlotDto>()
            : await _api.GetBookingSlotsAsync(offeringId, date, ct);

        var person = catalog.Resources.First(r => r.Offerings.Any(o => o.OfferingId == offeringId)).Name;
        return View(new BookPage(catalog, offering, person, y, m, date, days, slots));
    }

    [HttpPost]
    public async Task<IActionResult> Reserve(int offeringId, string date, string start, string? note, CancellationToken ct)
    {
        if (!Request.Cookies.ContainsKey(ClientAuth.TokenCookie))
        {
            TempData["BookingError"] = "برای رزرو وارد حساب کاربری شوید.";
            return RedirectToAction(nameof(Book), new { offeringId, date });
        }

        var result = await _api.ReserveBookingAsync(offeringId, date, start, note, ct);
        if (!result.Success)
        {
            TempData["BookingError"] = result.Error ?? "رزرو انجام نشد.";
            return RedirectToAction(nameof(Book), new { offeringId, date });
        }

        if (result.NeedsPayment && result.OrderId is int orderId)
            return RedirectToAction("Payment", "Checkout", new { orderId });

        TempData["BookingOk"] = "نوبت شما ثبت شد.";
        return RedirectToAction(nameof(Book), new { offeringId, date });
    }

    public sealed record BookPage(
        BookingCatalogDto Catalog,
        BookingCatalogOfferingDto Offering,
        string Person,
        int Year,
        int Month,
        string? Date,
        IReadOnlyList<BookingDayDto> Days,
        IReadOnlyList<BookingSlotDto> Slots);
}
