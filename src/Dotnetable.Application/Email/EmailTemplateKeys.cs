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
/// Tokens are replaced with <c>{{Token}}</c> placeholders; <c>SiteName</c>/<c>SiteUrl</c> are always
/// injected from the *sending* website, even when the template content itself is inherited.
/// </summary>
public static class EmailTemplateDefaults
{
    public static readonly IReadOnlyList<EmailTemplateDefault> All =
    [
        new(EmailTemplateKeys.AdminForgotPassword, "Admin — Forgot Password",
            "Reset your {{SiteName}} admin password",
            "<p>Hello {{Name}},</p>" +
            "<p>We received a request to reset your {{SiteName}} admin password. " +
            "Click the link below to choose a new password. This link expires in 30 minutes.</p>" +
            "<p><a href=\"{{ResetUrl}}\">{{ResetUrl}}</a></p>" +
            "<p>If you did not request this, you can safely ignore this email.</p>",
            EmailAccountType.NoReply),

        new(EmailTemplateKeys.ClientOtpActivation, "Customer — Activation Code",
            "Your {{SiteName}} activation code",
            "<p>Use the code below to activate your account on {{SiteName}}.</p>" +
            "<p style=\"font-size:24px;font-weight:bold;letter-spacing:3px\">{{Code}}</p>" +
            "<p>This code expires in 30 minutes. If you didn't request it, you can ignore this message.</p>",
            EmailAccountType.NoReply),

        new(EmailTemplateKeys.ClientOtpPasswordReset, "Customer — Password Reset Code",
            "Your {{SiteName}} password reset code",
            "<p>Use the code below to reset your password on {{SiteName}}.</p>" +
            "<p style=\"font-size:24px;font-weight:bold;letter-spacing:3px\">{{Code}}</p>" +
            "<p>This code expires in 30 minutes. If you didn't request it, you can ignore this message.</p>",
            EmailAccountType.NoReply),

        new(EmailTemplateKeys.Welcome, "Customer — Welcome",
            "Welcome to {{SiteName}}",
            "<p>Hello {{Name}},</p>" +
            "<p>Your account on {{SiteName}} is ready. We're glad to have you with us.</p>" +
            "<p><a href=\"{{SiteUrl}}\">{{SiteUrl}}</a></p>",
            EmailAccountType.NoReply),

        new(EmailTemplateKeys.OrderConfirmation, "Order Confirmation",
            "Your {{SiteName}} order #{{OrderNumber}} is confirmed",
            "<p>Hello {{Name}},</p>" +
            "<p>Thanks for your order! We've received order <strong>#{{OrderNumber}}</strong> " +
            "for a total of <strong>{{OrderTotal}}</strong>.</p>" +
            "<p>We'll let you know as soon as it ships.</p>",
            EmailAccountType.Sales),

        new(EmailTemplateKeys.Invoice, "Invoice",
            "Invoice for your {{SiteName}} order #{{OrderNumber}}",
            "<p>Hello {{Name}},</p>" +
            "<p>Please find the invoice for order <strong>#{{OrderNumber}}</strong> attached/linked below.</p>" +
            "<p><a href=\"{{InvoiceUrl}}\">{{InvoiceUrl}}</a></p>",
            EmailAccountType.Sales),

        new(EmailTemplateKeys.TicketReply, "Support Ticket Reply",
            "Re: {{TicketSubject}} — {{SiteName}} Support",
            "<p>Hello {{Name}},</p>" +
            "<p>{{ReplyBody}}</p>" +
            "<p><a href=\"{{TicketUrl}}\">View the full conversation</a></p>",
            EmailAccountType.Support),

        new(EmailTemplateKeys.Newsletter, "Newsletter",
            "{{NewsletterSubject}} — {{SiteName}}",
            "<p>{{NewsletterBody}}</p>" +
            "<p style=\"font-size:12px;color:#888\"><a href=\"{{UnsubscribeUrl}}\">Unsubscribe</a></p>",
            EmailAccountType.Marketing),

        new(EmailTemplateKeys.ContactFormNotification, "Contact Form Notification",
            "New contact message on {{SiteName}}",
            "<p>New message from <strong>{{Name}}</strong> ({{Email}}):</p>" +
            "<blockquote>{{MessageBody}}</blockquote>",
            EmailAccountType.Info),
    ];
}
