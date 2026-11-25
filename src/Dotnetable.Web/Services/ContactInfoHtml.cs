using System.Globalization;
using System.Net;
using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Enums;
using Microsoft.AspNetCore.Html;

namespace Dotnetable.Web.Services;

/// <summary>Renders one public contact row: phone and email become links, a location becomes an OpenStreetMap pin.</summary>
public static class ContactInfoHtml
{
    public static IHtmlContent Value(ContactInfoDto info)
    {
        var type = (ContactInfoType)info.Type;
        if (type == ContactInfoType.Location && info.Latitude is decimal lat && info.Longitude is decimal lng)
        {
            var latStr = lat.ToString("0.######", CultureInfo.InvariantCulture);
            var lngStr = lng.ToString("0.######", CultureInfo.InvariantCulture);
            var href = $"https://www.openstreetmap.org/?mlat={latStr}&mlon={lngStr}#map=17/{latStr}/{lngStr}";
            var label = string.IsNullOrWhiteSpace(info.Value) ? $"{latStr}, {lngStr}" : info.Value;
            return new HtmlString($"<a href=\"{WebUtility.HtmlEncode(href)}\" target=\"_blank\" rel=\"noopener\">{WebUtility.HtmlEncode(label)}</a>");
        }

        var scheme = type switch
        {
            ContactInfoType.Phone => "tel:",
            ContactInfoType.Email => "mailto:",
            _ => null,
        };

        if (scheme is null || string.IsNullOrWhiteSpace(info.Value))
            return new HtmlString(WebUtility.HtmlEncode(info.Value));

        return new HtmlString($"<a href=\"{WebUtility.HtmlEncode(scheme + info.Value.Trim())}\">{WebUtility.HtmlEncode(info.Value)}</a>");
    }

    public static string IconClass(ContactInfoDto info) =>
        string.IsNullOrWhiteSpace(info.Icon) ? "bi bi-info-circle" : info.Icon;
}
