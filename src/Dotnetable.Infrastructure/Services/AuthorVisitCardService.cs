using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.VisitCards;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class AuthorVisitCardService : IAuthorVisitCardService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ICacheService _cache;
    private readonly ICacheInvalidationNotifier _notifier;

    public AuthorVisitCardService(
        IDbContextFactory<AppDbContext> contextFactory,
        ICacheService cache,
        ICacheInvalidationNotifier notifier)
    {
        _contextFactory = contextFactory;
        _cache = cache;
        _notifier = notifier;
    }

    public async Task<AuthorVisitCardEditorState> GetEditorAsync(int memberId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var member = await _context.Members.AsNoTracking()
            .Where(m => m.MemberID == memberId)
            .Select(m => new
            {
                m.WebsiteID,
                m.Givenname,
                m.Surname,
                m.Username,
                m.Email,
                Address = m.Website.WebsiteAddress,
                Profile = m.AuthorProfile == null ? null : new
                {
                    m.AuthorProfile.DisplayName,
                    m.AuthorProfile.Headline,
                    m.AuthorProfile.PublicEmail,
                    m.AuthorProfile.WebsiteUrl,
                    m.AuthorProfile.Location,
                    m.AuthorProfile.SocialLinksJson,
                },
            })
            .FirstOrDefaultAsync(ct);
        if (member is null)
            return new AuthorVisitCardEditorState { MemberFound = false };

        var row = await _context.AuthorVisitCards.AsNoTracking()
            .Include(c => c.LogoFile)
            .Include(c => c.CoverFile)
            .FirstOrDefaultAsync(c => c.MemberID == memberId, ct);

        AuthorVisitCardEdit card;
        string? logoUrl = null;
        string? coverUrl = null;
        if (row is null)
        {
            var profile = member.Profile;
            card = new AuthorVisitCardEdit
            {
                PhoneBookEnabled = true,
                IsPortrait = true,
                FullName = profile?.DisplayName ?? $"{member.Givenname} {member.Surname}".Trim(),
                JobTitle = profile?.Headline,
                Email = profile?.PublicEmail ?? member.Email,
                Website = profile?.WebsiteUrl,
                Address = profile?.Location,
                Slug = VisitCardRules.SuggestSlug(member.Username, member.Givenname, member.Surname),
                SocialLinks = AuthorSocialLinks.Parse(profile?.SocialLinksJson),
            };
        }
        else
        {
            card = ToEdit(row);
            logoUrl = row.LogoFile is { IsDeleted: false } logo ? logo.ThumbnailCDN ?? logo.CNDUrl : null;
            coverUrl = row.CoverFile is { IsDeleted: false } cover ? cover.CNDUrl ?? cover.ThumbnailCDN : null;
        }

        return new AuthorVisitCardEditorState
        {
            MemberFound = true,
            WebsiteId = member.WebsiteID,
            WebsiteAddress = member.Address,
            LogoUrl = logoUrl,
            CoverUrl = coverUrl,
            Card = card,
        };
    }

    public async Task<string?> SaveAsync(int memberId, AuthorVisitCardEdit card, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var member = await _context.Members.AsNoTracking()
            .Where(m => m.MemberID == memberId)
            .Select(m => new { m.WebsiteID })
            .FirstOrDefaultAsync(ct);
        if (member is null) return "Member not found.";

        if (!VisitCardRules.TryNormalizeSlug(card.Slug, out var slug, out var slugError))
            return slugError;
        if (card.Enabled && slug is null)
            return "Choose a short link before publishing the card. Example: aliu";

        var bankError = VisitCardRules.PrepareBanks(card);
        if (bankError is not null) return bankError;

        if (slug is not null)
        {
            var taken = await _context.AuthorVisitCards.AnyAsync(c =>
                c.WebsiteID == member.WebsiteID && c.Slug == slug && c.MemberID != memberId, ct);
            if (taken) return "This short link is already used on this website.";
        }

        if (!await FileBelongsAsync(_context, card.LogoFileID, member.WebsiteID, ct))
            return "The logo file does not belong to this website.";
        if (!await FileBelongsAsync(_context, card.CoverFileID, member.WebsiteID, ct))
            return "The cover photo does not belong to this website.";

        var row = await _context.AuthorVisitCards.FirstOrDefaultAsync(c => c.MemberID == memberId, ct);
        var now = DateTime.UtcNow;
        if (row is null)
        {
            row = new AuthorVisitCard
            {
                MemberID = memberId,
                WebsiteID = member.WebsiteID,
                CreatedAt = now,
                IsPortrait = true,
                PhoneBookEnabled = true,
            };
            _context.AuthorVisitCards.Add(row);
        }

        row.Enabled = card.Enabled;
        row.PhoneBookEnabled = card.PhoneBookEnabled;
        row.Slug = slug;
        row.FullName = VisitCardRules.TrimTo(card.FullName, 70);
        row.BrandName = VisitCardRules.TrimTo(card.BrandName, 70);
        row.JobTitle = VisitCardRules.TrimTo(card.JobTitle, 60);
        row.Slogan = VisitCardRules.TrimTo(card.Slogan, 160);
        row.Mobile = EmptyToNull(VisitCardRules.NormalizePhone(card.Mobile), 20);
        row.Phone = EmptyToNull(VisitCardRules.NormalizePhone(card.Phone), 20);
        row.WhatsApp = EmptyToNull(VisitCardRules.NormalizePhone(card.WhatsApp), 20);
        row.Address = VisitCardRules.TrimTo(card.Address, 250);
        row.WebsiteUrl = VisitCardRules.NormalizeWebsite(card.Website);
        row.Email = VisitCardRules.TrimTo(card.Email, 120);
        row.LogoFileID = card.LogoFileID;
        row.CoverFileID = card.CoverFileID;
        row.CardColor = VisitCardRules.NormalizeColor(card.CardColor);
        row.PageColor = VisitCardRules.NormalizeColor(card.PageColor);
        row.Field1Title = VisitCardRules.TrimTo(card.Field1Title, 50);
        row.Field1Value = VisitCardRules.TrimTo(card.Field1Value, 250);
        row.Field2Title = VisitCardRules.TrimTo(card.Field2Title, 50);
        row.Field2Value = VisitCardRules.TrimTo(card.Field2Value, 250);
        row.Field3Title = VisitCardRules.TrimTo(card.Field3Title, 50);
        row.Field3Value = VisitCardRules.TrimTo(card.Field3Value, 250);
        row.Field4Title = VisitCardRules.TrimTo(card.Field4Title, 50);
        row.Field4Value = VisitCardRules.TrimTo(card.Field4Value, 250);
        row.Field5Title = VisitCardRules.TrimTo(card.Field5Title, 50);
        row.Field5Value = VisitCardRules.TrimTo(card.Field5Value, 250);
        row.IsPortrait = card.IsPortrait;
        row.CardLayout = card.CardLayout is 1 or 2 ? card.CardLayout : (byte)0;
        row.ThemeMode = card.ThemeMode == 1 ? (byte)1 : (byte)0;
        row.Description = VisitCardRules.TrimTo(card.Description, 4000);
        row.TranslationsJson = VisitCardRules.SerializeTranslations(card.Translations);
        row.SocialLinksJson = VisitCardRules.SerializeSocials(card.SocialLinks);
        row.LocationsJson = VisitCardRules.SerializeLocations(card.Locations);
        row.Bank1HolderName = card.Bank1HolderName;
        row.Bank1Details = card.Bank1Details;
        row.Bank1Sheba = card.Bank1Sheba;
        row.Bank1CardNumber = card.Bank1CardNumber;
        row.Bank2HolderName = card.Bank2HolderName;
        row.Bank2Details = card.Bank2Details;
        row.Bank2Sheba = card.Bank2Sheba;
        row.Bank2CardNumber = card.Bank2CardNumber;
        row.UpdatedAt = now;

        await _context.SaveChangesAsync(ct);
        await InvalidateAuthorCacheAsync(ct);
        return null;
    }

    public async Task<VisitCardPageDto?> GetPublicAsync(int websiteId, string slug, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        var row = await LoadPublicAsync(_context, websiteId, slug, ct);
        return row is null ? null : Project(row);
    }

    public async Task<string?> BuildPublicVcfAsync(int websiteId, string slug, CancellationToken ct = default)
    {
        var page = await GetPublicAsync(websiteId, slug, ct);
        if (page is null) return null;
        return VisitCardRules.BuildVcf(page.FullName, page.BrandName, page.JobTitle, page.Mobile, page.Phone,
            page.Email, page.Website, page.Address, page.Slogan);
    }

    public async Task<string?> InsertContactAsync(int websiteId, string slug, VisitCardContactRequest request, CancellationToken ct = default)
    {
        var code = VisitCardRules.ValidateContact(request.Name, request.Mobile, request.Email, request.Reason,
            out var name, out var mobile, out var email, out var reason);
        if (code is not null) return code;

        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        if (!VisitCardRules.TryNormalizeSlug(slug, out var normalized, out _) || normalized is null)
            return "missing";

        var card = await _context.AuthorVisitCards
            .FirstOrDefaultAsync(c => c.WebsiteID == websiteId && c.Slug == normalized && c.Enabled && c.Member.Active, ct);
        if (card is null) return "missing";
        if (!card.PhoneBookEnabled) return "disabled";

        var contacts = await _context.AuthorVisitCardContacts
            .Where(c => c.AuthorVisitCardID == card.AuthorVisitCardID)
            .ToListAsync(ct);

        AuthorVisitCardContact? existing = null;
        if (mobile is not null)
        {
            var key = VisitCardRules.MobileKey(mobile);
            existing = contacts
                .Where(c => VisitCardRules.MobileKey(c.Mobile) == key)
                .OrderByDescending(c => c.LogTime)
                .FirstOrDefault();
        }
        else if (email is not null)
        {
            existing = contacts
                .Where(c => string.Equals(c.Email, email, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(c => c.LogTime)
                .FirstOrDefault();
        }

        var visitor = VisitCardRules.NormalizeVisitorKey(request.VisitorKey);
        if (existing is null)
        {
            if (contacts.Count >= VisitCardRules.ContactCapacity) return "full";
            _context.AuthorVisitCardContacts.Add(new AuthorVisitCardContact
            {
                AuthorVisitCardID = card.AuthorVisitCardID,
                Name = name,
                Mobile = mobile,
                Email = email,
                Reason = reason,
                LogTime = DateTime.UtcNow,
                VisitorKey = visitor,
                SavedContact = request.SavedContact,
            });
        }
        else
        {
            existing.Reason = VisitCardRules.AppendReason(existing.Reason, reason);
            existing.LogTime = DateTime.UtcNow;
            if (email is not null) existing.Email = email;
            if (mobile is not null && string.IsNullOrWhiteSpace(existing.Mobile)) existing.Mobile = mobile;
            if (request.SavedContact) existing.SavedContact = true;
            if (visitor is not null) existing.VisitorKey = visitor;
        }

        await _context.SaveChangesAsync(ct);
        return null;
    }

    public async Task<VisitCardPhoneBookPage> ListContactsAsync(int memberId, string? search, bool onlySaved, int page, int pageSize, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 50 : pageSize;

        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        var card = await _context.AuthorVisitCards.AsNoTracking()
            .Where(c => c.MemberID == memberId)
            .Select(c => new { c.AuthorVisitCardID, c.PhoneBookEnabled, c.Slug })
            .FirstOrDefaultAsync(ct);
        if (card is null)
            return new VisitCardPhoneBookPage { Page = page, PageSize = pageSize };

        var q = _context.AuthorVisitCardContacts.AsNoTracking()
            .Where(c => c.AuthorVisitCardID == card.AuthorVisitCardID);
        if (onlySaved) q = q.Where(c => c.SavedContact);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(c =>
                c.Name.Contains(term) ||
                (c.DisplayName != null && c.DisplayName.Contains(term)) ||
                (c.Mobile != null && c.Mobile.Contains(term)) ||
                (c.Email != null && c.Email.Contains(term)) ||
                c.Reason.Contains(term) ||
                (c.Note != null && c.Note.Contains(term)));
        }

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(c => c.LogTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new VisitCardContactItem
            {
                ContactID = c.ContactID,
                Name = c.Name,
                DisplayName = c.DisplayName,
                Mobile = c.Mobile,
                Email = c.Email,
                Reason = c.Reason,
                Note = c.Note,
                SavedContact = c.SavedContact,
                LogTime = c.LogTime,
            })
            .ToListAsync(ct);

        return new VisitCardPhoneBookPage
        {
            HasCard = true,
            PhoneBookEnabled = card.PhoneBookEnabled,
            Slug = card.Slug,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            Items = items,
        };
    }

    public async Task<bool> UpdateContactAsync(int memberId, int contactId, string? displayName, string? note, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        var row = await OwnedContact(_context, memberId, contactId, ct);
        if (row is null) return false;
        row.DisplayName = VisitCardRules.TrimTo(displayName, 70);
        row.Note = VisitCardRules.TrimTo(note, 200);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteContactAsync(int memberId, int contactId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        var row = await OwnedContact(_context, memberId, contactId, ct);
        if (row is null) return false;
        _context.AuthorVisitCardContacts.Remove(row);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    private static async Task<AuthorVisitCardContact?> OwnedContact(AppDbContext context, int memberId, int contactId, CancellationToken ct) =>
        await context.AuthorVisitCardContacts
            .FirstOrDefaultAsync(c => c.ContactID == contactId && c.AuthorVisitCard.MemberID == memberId, ct);

    private static async Task<bool> FileBelongsAsync(AppDbContext context, int? fileId, int websiteId, CancellationToken ct)
    {
        if (fileId is null) return true;
        return await context.FileRecords.AnyAsync(f => f.FileRecordID == fileId && f.WebsiteID == websiteId && !f.IsDeleted, ct);
    }

    private static async Task<AuthorVisitCard?> LoadPublicAsync(AppDbContext context, int websiteId, string slug, CancellationToken ct)
    {
        if (!VisitCardRules.TryNormalizeSlug(slug, out var normalized, out _) || normalized is null)
            return null;
        return await context.AuthorVisitCards.AsNoTracking()
            .Include(c => c.LogoFile)
            .Include(c => c.CoverFile)
            .Include(c => c.Member).ThenInclude(m => m.AuthorProfile)
            .Include(c => c.Member).ThenInclude(m => m.Avatar)
            .Include(c => c.Website)
            .FirstOrDefaultAsync(c => c.WebsiteID == websiteId && c.Slug == normalized && c.Enabled && c.Member.Active, ct);
    }

    private static VisitCardPageDto Project(AuthorVisitCard row)
    {
        var logo = row.LogoFile is { IsDeleted: false } logoFile
            ? logoFile.CNDUrl ?? logoFile.ThumbnailCDN
            : row.Member.Avatar is { IsDeleted: false } avatar ? avatar.CNDUrl ?? avatar.ThumbnailCDN : null;

        return new VisitCardPageDto
        {
            Slug = row.Slug!,
            WebsiteName = row.Website.TradeName,
            ResumeSlug = row.Member.AuthorProfile is { ResumeEnabled: true } profile ? profile.Slug : null,
            PhoneBookEnabled = row.PhoneBookEnabled,
            IsPortrait = row.IsPortrait,
            CardLayout = row.CardLayout,
            ThemeMode = row.ThemeMode,
            CardColor = row.CardColor ?? "#1976d2",
            PageColor = row.PageColor ?? "#0d1b2a",
            FullName = row.FullName,
            BrandName = row.BrandName,
            JobTitle = row.JobTitle,
            Slogan = row.Slogan,
            Mobile = row.Mobile,
            Phone = row.Phone,
            WhatsApp = row.WhatsApp,
            Address = row.Address,
            Website = row.WebsiteUrl,
            Email = row.Email,
            LogoUrl = logo,
            CoverUrl = row.CoverFile is { IsDeleted: false } cover ? cover.CNDUrl ?? cover.ThumbnailCDN : null,
            Description = row.Description,
            Field1Title = row.Field1Title, Field1Value = row.Field1Value,
            Field2Title = row.Field2Title, Field2Value = row.Field2Value,
            Field3Title = row.Field3Title, Field3Value = row.Field3Value,
            Field4Title = row.Field4Title, Field4Value = row.Field4Value,
            Field5Title = row.Field5Title, Field5Value = row.Field5Value,
            Bank1HolderName = row.Bank1HolderName, Bank1Details = row.Bank1Details,
            Bank1Sheba = row.Bank1Sheba, Bank1CardNumber = row.Bank1CardNumber,
            Bank2HolderName = row.Bank2HolderName, Bank2Details = row.Bank2Details,
            Bank2Sheba = row.Bank2Sheba, Bank2CardNumber = row.Bank2CardNumber,
            SocialLinks = VisitCardRules.ParseSocials(row.SocialLinksJson),
            Locations = VisitCardRules.ParseLocations(row.LocationsJson),
            Translations = VisitCardRules.ParseTranslations(row.TranslationsJson),
        };
    }

    private static AuthorVisitCardEdit ToEdit(AuthorVisitCard row) => new()
    {
        Enabled = row.Enabled,
        PhoneBookEnabled = row.PhoneBookEnabled,
        Slug = row.Slug,
        FullName = row.FullName,
        BrandName = row.BrandName,
        JobTitle = row.JobTitle,
        Slogan = row.Slogan,
        Mobile = row.Mobile,
        Phone = row.Phone,
        WhatsApp = row.WhatsApp,
        Address = row.Address,
        Website = row.WebsiteUrl,
        Email = row.Email,
        LogoFileID = row.LogoFileID,
        CoverFileID = row.CoverFileID,
        CardColor = row.CardColor,
        PageColor = row.PageColor,
        Field1Title = row.Field1Title, Field1Value = row.Field1Value,
        Field2Title = row.Field2Title, Field2Value = row.Field2Value,
        Field3Title = row.Field3Title, Field3Value = row.Field3Value,
        Field4Title = row.Field4Title, Field4Value = row.Field4Value,
        Field5Title = row.Field5Title, Field5Value = row.Field5Value,
        IsPortrait = row.IsPortrait,
        CardLayout = row.CardLayout,
        ThemeMode = row.ThemeMode,
        Description = row.Description,
        Bank1HolderName = row.Bank1HolderName, Bank1Details = row.Bank1Details,
        Bank1Sheba = row.Bank1Sheba, Bank1CardNumber = row.Bank1CardNumber,
        Bank2HolderName = row.Bank2HolderName, Bank2Details = row.Bank2Details,
        Bank2Sheba = row.Bank2Sheba, Bank2CardNumber = row.Bank2CardNumber,
        Translations = VisitCardRules.ParseTranslations(row.TranslationsJson),
        SocialLinks = VisitCardRules.ParseSocials(row.SocialLinksJson),
        Locations = VisitCardRules.ParseLocations(row.LocationsJson),
    };

    private static string? EmptyToNull(string value, int max)
    {
        if (string.IsNullOrEmpty(value)) return null;
        return value.Length <= max ? value : value[..max];
    }

    private async Task InvalidateAuthorCacheAsync(CancellationToken ct)
    {
        _cache.RemoveByTag("author");
        await _notifier.NotifyAsync("author", ct);
    }
}
