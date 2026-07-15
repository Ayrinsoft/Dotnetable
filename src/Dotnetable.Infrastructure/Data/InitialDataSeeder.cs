using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Email;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Data;

/// <inheritdoc cref="IInitialDataSeeder"/>
public class InitialDataSeeder : IInitialDataSeeder
{
    private readonly IPasswordHasher<Member> _hasher;

    public InitialDataSeeder(IPasswordHasher<Member> hasher) => _hasher = hasher;

    public async Task SeedAsync(AppDbContext context, SetupRequest request, CancellationToken ct = default)
    {
        // Providers are configured with EnableRetryOnFailure, so a user-initiated transaction must
        // run inside the execution strategy as a single retriable unit — otherwise EF throws.
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            // 1. Default currency. Websites.DefaultCurrencyCode is a required FK into Currencies,
            // so the chosen currency must exist before the website row is inserted.
            var currencyCode = request.DefaultCurrencyCode.Trim().ToUpperInvariant();
            var currency = await context.Currencies.FindAsync(new object[] { currencyCode }, ct);
            if (currency is null)
            {
                currency = new Currency
                {
                    CurrencyCode = currencyCode,
                    Name = request.CurrencyName,
                    Symbol = request.CurrencySymbol,
                    DecimalDigits = request.CurrencyDecimalDigits,
                    IsActive = true,
                };
                context.Currencies.Add(currency);
                await context.SaveChangesAsync(ct);
            }

            // 2. Master website. As the first row inserted into an empty table it receives WebsiteID 1.
            var website = new Website
            {
                TradeName = request.TradeName,
                BrandName = request.BrandName,
                WebsiteAddress = request.WebsiteAddress,
                Manager = request.Manager,
                Mobile = request.Mobile,
                Email = request.WebsiteEmail,
                DefaultLanguageCode = request.DefaultLanguageCode,
                DefaultCurrencyCode = currencyCode,
                RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
                AuthCode = Guid.NewGuid(),
                Active = true,
                AllowAllIP = true,
                WebsiteType = (byte)WebsiteType.Corporate,
            };
            context.Websites.Add(website);

            foreach (var featureKey in WebsiteType.Corporate.GetDefaultFeatures())
            {
                context.WebsiteFeatures.Add(new WebsiteFeature { Website = website, FeatureKey = (byte)featureKey, Enabled = true });
            }

            await context.SaveChangesAsync(ct);

            // 2a. Master language catalog — every other website picks a subset of these for its
            // own content later, from the /languages admin page.
            var languageDefaults = new (string Code, string Iso, string Name, bool Rtl)[]
            {
                ("en", "en-US", "English",  false),
                ("de", "de-DE", "Deutsch",  false),
                ("fr", "fr-FR", "Français", false),
                ("ru", "ru-RU", "Русский",  false),
                ("zh", "zh-CN", "中文",      false),
                ("fa", "fa-IR", "فارسی",    true),
                ("ar", "ar-SA", "العربية",  true),
            };
            context.Languages.AddRange(languageDefaults.Select((l, i) => new Language
            {
                WebsiteID = website.WebsiteID,
                LanguageCode = l.Code,
                LanguageCodeISO = l.Iso,
                Name = l.Name,
                Priority = i,
                Active = true,
                IsDefault = l.Code == request.DefaultLanguageCode || (l.Code == "en" && languageDefaults.All(d => d.Code != request.DefaultLanguageCode)),
                RTLDesign = l.Rtl,
            }));
            await context.SaveChangesAsync(ct);

            // 4. Seed every permission (admin + client) from the catalog.
            var roles = RoleCatalog.All
                .Select(def => new Role
                {
                    RoleKey = def.Key,
                    Description = def.Description,
                    Category = (byte)def.Category,
                    Active = true,
                })
                .ToList();
            context.Roles.AddRange(roles);
            await context.SaveChangesAsync(ct);

            // 4a. Super-administrator policy for the master website — granted every permission (full access).
            var policy = new Policy { Title = DefaultPolicies.Administrators, Active = true, WebsiteID = website.WebsiteID };
            context.Policies.Add(policy);
            await context.SaveChangesAsync(ct);

            context.PolicyRoles.AddRange(roles.Select(r => new PolicyRole
            {
                PolicyID = policy.PolicyID,
                RoleID = r.RoleID,
                Active = true,
            }));
            await context.SaveChangesAsync(ct);

            // 4b. Default customer ("Users") policy — sign-in/general access, commenting and purchasing.
            // Self-registered members on this website receive this policy.
            var usersPolicy = new Policy { Title = DefaultPolicies.Users, Active = true, WebsiteID = website.WebsiteID };
            context.Policies.Add(usersPolicy);
            await context.SaveChangesAsync(ct);

            context.PolicyRoles.AddRange(roles
                .Where(r => r.Category == (byte)RoleCategory.Client)
                .Select(r => new PolicyRole
                {
                    PolicyID = usersPolicy.PolicyID,
                    RoleID = r.RoleID,
                    Active = true,
                }));
            await context.SaveChangesAsync(ct);

            // 5. First administrator member, bound to the master website.
            var member = new Member
            {
                WebsiteID = website.WebsiteID,
                PolicyID = policy.PolicyID,
                Username = request.Username,
                Email = request.Email,
                Givenname = request.Givenname,
                Surname = request.Surname,
                CellphoneNumber = string.Empty,
                CountryCode = string.Empty,
                RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
                HashKey = Guid.NewGuid(),
                Active = true,
            };
            member.Password = _hasher.HashPassword(member, request.Password);
            context.Members.Add(member);
            await context.SaveChangesAsync(ct);

            // 6. Optional default SMTP account so forgot-password email works from first run. Every
            // other website falls back to this NoReply account until it registers its own.
            if (!string.IsNullOrWhiteSpace(request.MailServer) && !string.IsNullOrWhiteSpace(request.MailAddress))
            {
                context.EmailAccounts.Add(new EmailAccount
                {
                    WebsiteID = website.WebsiteID,
                    AccountType = (byte)EmailAccountType.NoReply,
                    Name = "No-Reply",
                    MailServer = request.MailServer.Trim(),
                    SMTPPort = request.SmtpPort,
                    EnableSSL = request.MailEnableSSL,
                    EmailAddress = request.MailAddress.Trim(),
                    Password = request.MailPassword,
                    MailName = string.IsNullOrWhiteSpace(request.MailName) ? request.MailAddress.Trim() : request.MailName.Trim(),
                    IsDefault = true,
                    Active = true,
                });
                await context.SaveChangesAsync(ct);
            }

            // 6a. Default email templates for the master website — every other website falls back to
            // these (by TemplateKey) until it saves its own override.
            context.EmailTemplates.AddRange(EmailTemplateDefaults.All.Select(def => new EmailTemplate
            {
                WebsiteID = website.WebsiteID,
                TemplateKey = def.Key,
                Name = def.Name,
                Subject = def.Subject,
                HtmlBody = def.HtmlBody,
                AccountType = (byte)def.AccountType,
                Active = true,
            }));
            await context.SaveChangesAsync(ct);

            // 7. Default CMS pages so a fresh install has real, admin-editable About/Contact/Services
            // pages instead of the hardcoded demo views the site theme used to render.
            context.Pages.AddRange(
                new Page
                {
                    WebsiteID = website.WebsiteID,
                    Slug = "about-us",
                    Title = "About Us",
                    Content = "<p>Tell visitors who you are and what you do. Edit this page any time from Content → Pages.</p>",
                    Status = 1,
                    IsActive = true,
                },
                new Page
                {
                    WebsiteID = website.WebsiteID,
                    Slug = "contact-us",
                    Title = "Contact Us",
                    Content = "<p>Share how visitors can reach you. A contact form is shown automatically below this content.</p>",
                    Status = 1,
                    IsActive = true,
                },
                new Page
                {
                    WebsiteID = website.WebsiteID,
                    Slug = "services",
                    Title = "Services",
                    Content = "<p>Describe what you offer. Edit this page any time from Content → Pages.</p>",
                    Status = 1,
                    IsActive = true,
                });
            await context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
        });
    }
}
