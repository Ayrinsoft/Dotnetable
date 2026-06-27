using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
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

            // 1. Master website. As the first row inserted into an empty table it receives WebsiteID 1.
            var website = new Website
            {
                TradeName = request.TradeName,
                BrandName = request.BrandName,
                WebsiteAddress = request.WebsiteAddress,
                Manager = request.Manager,
                Mobile = request.Mobile,
                Email = request.WebsiteEmail,
                DefaultLanguageCode = request.DefaultLanguageCode,
                RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
                AuthCode = Guid.NewGuid(),
                Active = true,
                AllowAllIP = true,
                WebsiteType = 0,
            };
            context.Websites.Add(website);
            await context.SaveChangesAsync(ct);

            // 2. Seed every permission (admin + client) from the catalog.
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

            // 3a. Super-administrator policy for the master website — granted every permission (full access).
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

            // 3b. Default customer ("Users") policy — sign-in/general access, commenting and purchasing.
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

            // 4. First administrator member, bound to the master website.
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

            // 5. Optional default SMTP settings so forgot-password email works from first run.
            if (!string.IsNullOrWhiteSpace(request.MailServer) && !string.IsNullOrWhiteSpace(request.MailAddress))
            {
                context.EmailSettings.Add(new EmailSetting
                {
                    MailServer = request.MailServer.Trim(),
                    SMTPPort = request.SmtpPort,
                    EnableSSL = request.MailEnableSSL,
                    EmailAddress = request.MailAddress.Trim(),
                    Password = request.MailPassword,
                    MailName = string.IsNullOrWhiteSpace(request.MailName) ? request.MailAddress.Trim() : request.MailName.Trim(),
                    EmailTypeID = 0,
                    DefaultEMail = true,
                    Active = true,
                });
                await context.SaveChangesAsync(ct);
            }

            await transaction.CommitAsync(ct);
        });
    }
}
