using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Email;

/// <summary>Well-known <c>EmailTemplate.TemplateKey</c> values referenced from code.</summary>
public static class EmailTemplateKeys
{
    public const string AdminForgotPassword = "AdminForgotPassword";
    public const string ClientOtpActivation = "ClientOtpActivation";
    public const string ClientOtpPasswordReset = "ClientOtpPasswordReset";
    public const string Welcome = "Welcome";
    public const string OrderConfirmation = "OrderConfirmation";
    public const string Invoice = "Invoice";
    public const string TicketReply = "TicketReply";
    public const string Newsletter = "Newsletter";
    public const string ContactFormNotification = "ContactFormNotification";
}

/// <summary>One built-in template shipped as the master website's default row.</summary>
public sealed record EmailTemplateDefault(string Key, string Name, string Subject, string HtmlBody, EmailAccountType AccountType);

/// <summary>
/// The default subject/body seeded for the master website (<c>AppConstants.MasterWebsiteId</c>) on
/// first run. Every other website falls back to these until it saves its own override.
/// Tokens are replaced with <c>{{Token}}</c> placeholders; <c>SiteName</c>/<c>SiteUrl</c>/<c>Year</c>
/// are always injected from the *sending* website, even when the template content itself is inherited.
///
/// All bodies share the <see cref="Layout"/> shell so every outgoing email looks like one product —
/// same header bar, accent color and footer as the admin panel theme (<c>#348fe2</c> accent on a
/// <c>#2d353c</c> dark header, card on a light background).
/// </summary>
public static class EmailTemplateDefaults
{
    // Mirrors the admin panel palette (Dotnetable.Admin/wwwroot/css/admin-theme.css and MainLayout.razor).
    private const string AccentColor = "#348fe2";
    private const string DarkColor = "#2d353c";
    private const string BodyBg = "#e4e7ea";
    private const string CardBg = "#ffffff";
    private const string PanelBg = "#f4f6f7";
    private const string BorderColor = "#e2e7eb";
    private const string TextMuted = "#7b8488";
    private const string FontFamily = "'Segoe UI', 'Open Sans', Arial, sans-serif";

    /// <summary>Wraps inner content HTML in the shared, table-based, email-safe layout shell.</summary>
    private static string Layout(string title, string innerHtml) =>
        "<!doctype html>" +
        "<html><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">" +
        $"<title>{title}</title></head>" +
        $"<body style=\"margin:0;padding:0;background-color:{BodyBg};font-family:{FontFamily};\">" +
        $"<div style=\"background-color:{BodyBg};padding:32px 16px;\">" +
        "<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\"><tr><td align=\"center\">" +
        $"<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width:560px;background-color:{CardBg};border:1px solid {BorderColor};border-radius:8px;overflow:hidden;\">" +
        "<tr><td style=\"padding:22px 32px;background-color:" + DarkColor + ";\">" +
        "<span style=\"color:#ffffff;font-size:18px;font-weight:600;letter-spacing:.3px;\">{{SiteName}}</span>" +
        "</td></tr>" +
        $"<tr><td style=\"height:4px;line-height:4px;font-size:0;background-color:{AccentColor};\">&nbsp;</td></tr>" +
        $"<tr><td style=\"padding:32px;color:{DarkColor};font-size:15px;line-height:1.65;\">" +
        innerHtml +
        "</td></tr>" +
        $"<tr><td style=\"padding:18px 32px;background-color:{PanelBg};border-top:1px solid {BorderColor};\">" +
        $"<p style=\"margin:0;color:{TextMuted};font-size:12px;\">&copy; {{{{Year}}}} {{{{SiteName}}}} &middot; <a href=\"{{{{SiteUrl}}}}\" style=\"color:{TextMuted};text-decoration:underline;\">{{{{SiteUrl}}}}</a></p>" +
        "</td></tr>" +
        "</table>" +
        "</td></tr></table>" +
        "</div></body></html>";

    private static string Button(string url, string text) =>
        "<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" style=\"margin:22px 0;\"><tr>" +
        $"<td style=\"background-color:{AccentColor};border-radius:4px;\">" +
        $"<a href=\"{url}\" style=\"display:inline-block;padding:12px 28px;color:#ffffff;font-size:14px;font-weight:600;text-decoration:none;\">{text}</a>" +
        "</td></tr></table>";

    private static string CodeBox(string code) =>
        $"<div style=\"margin:22px 0;text-align:center;\"><span style=\"display:inline-block;background-color:#eaf3fc;color:{AccentColor};font-size:28px;font-weight:700;letter-spacing:6px;padding:14px 30px;border-radius:6px;border:1px dashed {AccentColor};\">{code}</span></div>";

    private static string Quote(string html) =>
        $"<div style=\"background-color:{PanelBg};border-left:3px solid {AccentColor};padding:14px 18px;margin:0 0 16px;color:{DarkColor};font-size:14px;border-radius:0 4px 4px 0;\">{html}</div>";

    public static readonly IReadOnlyList<EmailTemplateDefault> All =
    [
        new(EmailTemplateKeys.AdminForgotPassword, "Admin — Forgot Password",
            "Reset your {{SiteName}} admin password",
            Layout("Reset your password",
                "<p style=\"margin:0 0 12px;\">Hello <strong>{{Name}}</strong>,</p>" +
                "<p style=\"margin:0 0 12px;\">We received a request to reset your {{SiteName}} admin password. Click the button below to choose a new password. This link expires in 30 minutes.</p>" +
                Button("{{ResetUrl}}", "Reset Password") +
                $"<p style=\"margin:16px 0 0;color:{TextMuted};font-size:13px;\">If the button doesn't work, copy and paste this link into your browser:<br>" +
                $"<a href=\"{{{{ResetUrl}}}}\" style=\"color:{AccentColor};word-break:break-all;\">{{{{ResetUrl}}}}</a></p>" +
                $"<p style=\"margin:20px 0 0;color:{TextMuted};font-size:13px;\">If you did not request this, you can safely ignore this email.</p>"),
            EmailAccountType.NoReply),

        new(EmailTemplateKeys.ClientOtpActivation, "Customer — Activation Code",
            "Your {{SiteName}} activation code",
            Layout("Your activation code",
                "<p style=\"margin:0;\">Use the code below to activate your account on {{SiteName}}.</p>" +
                CodeBox("{{Code}}") +
                $"<p style=\"margin:0;color:{TextMuted};font-size:13px;text-align:center;\">This code expires in 30 minutes. If you didn't request it, you can ignore this message.</p>"),
            EmailAccountType.NoReply),

        new(EmailTemplateKeys.ClientOtpPasswordReset, "Customer — Password Reset Code",
            "Your {{SiteName}} password reset code",
            Layout("Your password reset code",
                "<p style=\"margin:0;\">Use the code below to reset your password on {{SiteName}}.</p>" +
                CodeBox("{{Code}}") +
                $"<p style=\"margin:0;color:{TextMuted};font-size:13px;text-align:center;\">This code expires in 30 minutes. If you didn't request it, you can ignore this message.</p>"),
            EmailAccountType.NoReply),

        new(EmailTemplateKeys.Welcome, "Customer — Welcome",
            "Welcome to {{SiteName}}",
            Layout("Welcome",
                "<p style=\"margin:0 0 12px;\">Hello <strong>{{Name}}</strong>,</p>" +
                "<p style=\"margin:0 0 12px;\">Your account on {{SiteName}} is ready. We're glad to have you with us.</p>" +
                Button("{{SiteUrl}}", "Visit {{SiteName}}")),
            EmailAccountType.NoReply),

        new(EmailTemplateKeys.OrderConfirmation, "Order Confirmation",
            "Your {{SiteName}} order #{{OrderNumber}} is confirmed",
            Layout("Order confirmed",
                "<p style=\"margin:0 0 12px;\">Hello <strong>{{Name}}</strong>,</p>" +
                "<p style=\"margin:0 0 18px;\">Thanks for your order! We've received it and it's now being processed.</p>" +
                $"<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background-color:{PanelBg};border-radius:6px;margin:0 0 18px;\"><tr><td style=\"padding:16px 20px;\">" +
                "<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\">" +
                $"<tr><td style=\"color:{TextMuted};font-size:13px;padding:4px 0;\">Order Number</td><td align=\"right\" style=\"color:{DarkColor};font-size:13px;font-weight:600;\">#{{{{OrderNumber}}}}</td></tr>" +
                $"<tr><td style=\"color:{TextMuted};font-size:13px;padding:4px 0;\">Total</td><td align=\"right\" style=\"color:{AccentColor};font-size:13px;font-weight:600;\">{{{{OrderTotal}}}}</td></tr>" +
                "</table></td></tr></table>" +
                $"<p style=\"margin:0;color:{TextMuted};font-size:13px;\">We'll let you know as soon as it ships.</p>"),
            EmailAccountType.Sales),

        new(EmailTemplateKeys.Invoice, "Invoice",
            "Invoice for your {{SiteName}} order #{{OrderNumber}}",
            Layout("Invoice",
                "<p style=\"margin:0 0 12px;\">Hello <strong>{{Name}}</strong>,</p>" +
                "<p style=\"margin:0 0 4px;\">Please find the invoice for order <strong>#{{OrderNumber}}</strong> below.</p>" +
                Button("{{InvoiceUrl}}", "View Invoice")),
            EmailAccountType.Sales),

        new(EmailTemplateKeys.TicketReply, "Support Ticket Reply",
            "Re: {{TicketSubject}} — {{SiteName}} Support",
            Layout("Support ticket reply",
                "<p style=\"margin:0 0 12px;\">Hello <strong>{{Name}}</strong>,</p>" +
                Quote("{{ReplyBody}}") +
                Button("{{TicketUrl}}", "View Full Conversation")),
            EmailAccountType.Support),

        new(EmailTemplateKeys.Newsletter, "Newsletter",
            "{{NewsletterSubject}} — {{SiteName}}",
            Layout("{{NewsletterSubject}}",
                "<div style=\"font-size:14px;\">{{NewsletterBody}}</div>" +
                $"<p style=\"margin:20px 0 0;padding-top:16px;border-top:1px solid {BorderColor};color:#9aa1a6;font-size:12px;\">" +
                $"<a href=\"{{{{UnsubscribeUrl}}}}\" style=\"color:#9aa1a6;\">Unsubscribe</a> from these emails.</p>"),
            EmailAccountType.Marketing),

        new(EmailTemplateKeys.ContactFormNotification, "Contact Form Notification",
            "New contact message on {{SiteName}}",
            Layout("New contact message",
                $"<p style=\"margin:0 0 12px;\">New message from <strong>{{{{Name}}}}</strong> (<a href=\"mailto:{{{{Email}}}}\" style=\"color:{AccentColor};\">{{{{Email}}}}</a>):</p>" +
                Quote("{{MessageBody}}")),
            EmailAccountType.Info),
    ];
}
