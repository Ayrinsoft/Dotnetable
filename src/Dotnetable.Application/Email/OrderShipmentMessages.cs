namespace Dotnetable.Application.Email;

/// <summary>
/// Plain-text shipment / tracking messages for SMS &amp; WhatsApp, localized to the website default language.
/// Email HTML lives in <see cref="EmailTemplateDefaults"/> / <see cref="EmailTemplateKeys.OrderShipped"/>.
/// </summary>
public static class OrderShipmentMessages
{
    public static bool IsPersian(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode)) return false;
        var lang = languageCode.Trim().ToLowerInvariant();
        return lang is "fa" or "fa-ir" or "per" or "persian" || lang.StartsWith("fa-", StringComparison.Ordinal);
    }

    /// <summary>SMS / WhatsApp body in the site default language.</summary>
    public static string PlainText(
        string? languageCode,
        string siteName,
        string customerName,
        string orderNumber,
        string trackingCode,
        string? shippingMethod)
    {
        siteName = string.IsNullOrWhiteSpace(siteName) ? "Shop" : siteName.Trim();
        customerName = string.IsNullOrWhiteSpace(customerName) ? (IsPersian(languageCode) ? "مشتری" : "Customer") : customerName.Trim();
        orderNumber = orderNumber.Trim();
        trackingCode = trackingCode.Trim();
        var method = string.IsNullOrWhiteSpace(shippingMethod) ? null : shippingMethod.Trim();

        if (IsPersian(languageCode))
        {
            var methodPart = method is null ? "" : $"\nروش ارسال: {method}";
            return
                $"{customerName} عزیز، سفارش #{orderNumber} از {siteName} ارسال شد.{methodPart}\n" +
                $"کد پیگیری پستی/ارسال: {trackingCode}\n" +
                "می‌توانید وضعیت ارسال را در حساب کاربری خود نیز ببینید.";
        }

        {
            var methodPart = method is null ? "" : $"\nShipping method: {method}";
            return
                $"Hello {customerName}, your order #{orderNumber} from {siteName} has shipped.{methodPart}\n" +
                $"Tracking code: {trackingCode}\n" +
                "You can also check shipping status in your account.";
        }
    }

    /// <summary>Built-in email subject/body when no DB override/translation exists for the language.</summary>
    public static bool TryGetEmailDefault(string? languageCode, out string subject, out string htmlInner)
    {
        if (IsPersian(languageCode))
        {
            subject = "سفارش #{{OrderNumber}} از {{SiteName}} ارسال شد — کد پیگیری {{TrackingCode}}";
            htmlInner =
                "<p style=\"margin:0 0 12px;\">{{Name}} عزیز،</p>" +
                "<p style=\"margin:0 0 18px;\">سفارش شما ارسال شد. جزئیات زیر است:</p>" +
                "<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background-color:#f4f6f7;border-radius:6px;margin:0 0 18px;\"><tr><td style=\"padding:16px 20px;\">" +
                "<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\">" +
                "<tr><td style=\"color:#7b8488;font-size:13px;padding:4px 0;\">شماره سفارش</td><td align=\"right\" style=\"color:#2d353c;font-size:13px;font-weight:600;\">#{{OrderNumber}}</td></tr>" +
                "<tr><td style=\"color:#7b8488;font-size:13px;padding:4px 0;\">کد پیگیری</td><td align=\"right\" style=\"color:#348fe2;font-size:13px;font-weight:700;letter-spacing:0.5px;\">{{TrackingCode}}</td></tr>" +
                "<tr><td style=\"color:#7b8488;font-size:13px;padding:4px 0;\">روش ارسال</td><td align=\"right\" style=\"color:#2d353c;font-size:13px;font-weight:600;\">{{ShippingMethod}}</td></tr>" +
                "</table></td></tr></table>" +
                "<p style=\"margin:0;color:#7b8488;font-size:13px;\">وضعیت آماده‌سازی و ارسال را در حساب کاربری خود دنبال کنید.</p>";
            return true;
        }

        subject = "Your {{SiteName}} order #{{OrderNumber}} has shipped — tracking {{TrackingCode}}";
        htmlInner =
            "<p style=\"margin:0 0 12px;\">Hello <strong>{{Name}}</strong>,</p>" +
            "<p style=\"margin:0 0 18px;\">Great news — your order is on its way. Details below:</p>" +
            "<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background-color:#f4f6f7;border-radius:6px;margin:0 0 18px;\"><tr><td style=\"padding:16px 20px;\">" +
            "<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\">" +
            "<tr><td style=\"color:#7b8488;font-size:13px;padding:4px 0;\">Order Number</td><td align=\"right\" style=\"color:#2d353c;font-size:13px;font-weight:600;\">#{{OrderNumber}}</td></tr>" +
            "<tr><td style=\"color:#7b8488;font-size:13px;padding:4px 0;\">Tracking code</td><td align=\"right\" style=\"color:#348fe2;font-size:13px;font-weight:700;letter-spacing:0.5px;\">{{TrackingCode}}</td></tr>" +
            "<tr><td style=\"color:#7b8488;font-size:13px;padding:4px 0;\">Shipping method</td><td align=\"right\" style=\"color:#2d353c;font-size:13px;font-weight:600;\">{{ShippingMethod}}</td></tr>" +
            "</table></td></tr></table>" +
            "<p style=\"margin:0;color:#7b8488;font-size:13px;\">You can track preparation and shipping status in your account.</p>";
        return true;
    }
}
