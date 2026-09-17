using System.Text.Json;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Text;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class AuthorProfileService : IAuthorProfileService
{
    private const int SlugMaxLength = 100;

    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public AuthorProfileService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    // ── Admin management ────────────────────────────────────────────

    public async Task<List<AuthorListRow>> GetAuthorsAsync(int? websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.Members.AsNoTracking().AsQueryable();
        if (websiteId is int wid)
            q = q.Where(m => m.WebsiteID == wid);

        return await q
            .OrderBy(m => m.Givenname).ThenBy(m => m.Surname)
            .Select(m => new AuthorListRow
            {
                MemberID = m.MemberID,
                Username = m.Username,
                FullName = (m.Givenname + " " + m.Surname).Trim(),
                Active = m.Active,
                WebsiteID = m.WebsiteID,
                HasProfile = m.AuthorProfile != null,
                Slug = m.AuthorProfile != null ? m.AuthorProfile.Slug : null,
                ResumeEnabled = m.AuthorProfile != null && m.AuthorProfile.ResumeEnabled,
                HasBio = m.AuthorProfile != null && m.AuthorProfile.Bio != null && m.AuthorProfile.Bio != "",
                ResumeItemCount = m.AuthorProfile != null ? m.AuthorProfile.AuthorResumeItems.Count : 0,
                PostCount = m.Posts.Count,
            })
            .ToListAsync(ct);
    }

    public async Task<AuthorProfile?> GetByMemberAsync(int memberId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.AuthorProfiles.AsNoTracking()
            .Include(p => p.PhotoFile)
            .Include(p => p.ResumeFile)
            .Include(p => p.AuthorProfileTranslations)
            .Include(p => p.AuthorResumeItems).ThenInclude(i => i.AuthorResumeItemTranslations)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.MemberID == memberId, ct);
    }

    public async Task<AuthorProfile> SaveAsync(AuthorProfile profile, IReadOnlyList<AuthorProfileTranslation> translations, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var member = await _context.Members.AsNoTracking()
            .Where(m => m.MemberID == profile.MemberID)
            .Select(m => new { m.WebsiteID, m.Givenname, m.Surname, m.Username })
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("Member not found.");

        var row = await _context.AuthorProfiles
            .Include(p => p.AuthorProfileTranslations)
            .FirstOrDefaultAsync(p => p.MemberID == profile.MemberID, ct);

        var now = DateTime.UtcNow;
        if (row is null)
        {
            row = new AuthorProfile { MemberID = profile.MemberID, CreatedAt = now };
            _context.AuthorProfiles.Add(row);
        }

        // The website always follows the member: a profile can never be published on another site.
        row.WebsiteID = member.WebsiteID;

        var slugSource = !string.IsNullOrWhiteSpace(profile.Slug) ? profile.Slug
            : !string.IsNullOrWhiteSpace(profile.DisplayName) ? profile.DisplayName
            : $"{member.Givenname} {member.Surname}".Trim() is { Length: > 0 } fullName ? fullName
            : member.Username;
        var baseSlug = SlugGenerator.Normalize(slugSource, SlugMaxLength);
        // Normalize answers "item" for input that is all punctuation; an author URL deserves better.
        if (baseSlug == "item") baseSlug = $"author-{profile.MemberID}";
        var usedSlugs = (await _context.AuthorProfiles.AsNoTracking()
                .Where(p => p.WebsiteID == member.WebsiteID && p.MemberID != profile.MemberID)
                .Select(p => p.Slug).ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        row.Slug = SlugGenerator.MakeUnique(baseSlug, usedSlugs);

        row.DisplayName = Clean(profile.DisplayName);
        row.Headline = Clean(profile.Headline);
        row.Bio = Clean(profile.Bio);
        row.ShowBioOnPosts = profile.ShowBioOnPosts;
        row.ResumeEnabled = profile.ResumeEnabled;
        row.About = Clean(profile.About);
        row.Location = Clean(profile.Location);
        row.PublicEmail = Clean(profile.PublicEmail);
        row.WebsiteUrl = AuthorSocialLinks.IsSafeUrl(profile.WebsiteUrl) ? profile.WebsiteUrl!.Trim() : null;
        row.SocialLinksJson = AuthorSocialLinks.Serialize(AuthorSocialLinks.Parse(profile.SocialLinksJson));
        row.Skills = NormalizeSkills(profile.Skills);
        // Both files are published on the storefront, so only this website's own, non-deleted files.
        row.PhotoFileID = await OwnFileAsync(_context, profile.PhotoFileID, member.WebsiteID, ct);
        row.ResumeFileID = await OwnFileAsync(_context, profile.ResumeFileID, member.WebsiteID, ct);
        row.UpdatedAt = now;

        SyncTranslations(_context, row, translations);

        await _context.SaveChangesAsync(ct);
        return row;
    }

    public async Task<AuthorResumeItem> SaveResumeItemAsync(int memberId, AuthorResumeItem item, IReadOnlyList<AuthorResumeItemTranslation> translations, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(item.Title))
            throw new InvalidOperationException("A résumé item needs a title.");

        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var profileId = await _context.AuthorProfiles
            .Where(p => p.MemberID == memberId)
            .Select(p => (int?)p.AuthorProfileID)
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("Save the author profile before adding résumé items.");

        AuthorResumeItem row;
        if (item.AuthorResumeItemID == 0)
        {
            row = new AuthorResumeItem { AuthorProfileID = profileId };
            _context.AuthorResumeItems.Add(row);
        }
        else
        {
            row = await _context.AuthorResumeItems
                .Include(i => i.AuthorResumeItemTranslations)
                .FirstOrDefaultAsync(i => i.AuthorResumeItemID == item.AuthorResumeItemID && i.AuthorProfileID == profileId, ct)
                ?? throw new InvalidOperationException("Résumé item not found.");
        }

        row.ItemType = Enum.IsDefined(typeof(ResumeItemType), item.ItemType) ? item.ItemType : (byte)ResumeItemType.Other;
        row.Title = item.Title.Trim();
        row.Organization = Clean(item.Organization);
        row.Location = Clean(item.Location);
        row.Description = Clean(item.Description);
        row.Url = AuthorSocialLinks.IsSafeUrl(item.Url) ? item.Url!.Trim() : null;
        row.StartDate = item.StartDate;
        row.IsCurrent = item.IsCurrent;
        row.EndDate = item.IsCurrent ? null : item.EndDate;
        if (row.StartDate is DateOnly s && row.EndDate is DateOnly e && e < s)
            throw new InvalidOperationException("The end date cannot be before the start date.");
        row.ShowInTimeline = item.ShowInTimeline;
        row.SortOrder = item.SortOrder;
        row.IsActive = item.IsActive;

        var wanted = translations
            .Where(t => !string.IsNullOrWhiteSpace(t.LanguageCode)
                        && (HasText(t.Title) || HasText(t.Organization) || HasText(t.Location) || HasText(t.Description)))
            .GroupBy(t => t.LanguageCode.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First());
        foreach (var existing in row.AuthorResumeItemTranslations.Where(t => !wanted.ContainsKey(t.LanguageCode.Trim().ToLowerInvariant())).ToList())
            _context.AuthorResumeItemTranslations.Remove(existing);
        foreach (var (code, t) in wanted)
        {
            var current = row.AuthorResumeItemTranslations.FirstOrDefault(x => string.Equals(x.LanguageCode.Trim(), code, StringComparison.OrdinalIgnoreCase));
            if (current is null)
            {
                current = new AuthorResumeItemTranslation { LanguageCode = code };
                row.AuthorResumeItemTranslations.Add(current);
            }
            current.Title = Clean(t.Title);
            current.Organization = Clean(t.Organization);
            current.Location = Clean(t.Location);
            current.Description = Clean(t.Description);
        }

        await TouchProfileAsync(_context, profileId, ct);
        await _context.SaveChangesAsync(ct);
        return row;
    }

    public async Task DeleteResumeItemAsync(int memberId, int itemId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var row = await _context.AuthorResumeItems
            .Include(i => i.AuthorResumeItemTranslations)
            .FirstOrDefaultAsync(i => i.AuthorResumeItemID == itemId && i.AuthorProfile.MemberID == memberId, ct);
        if (row is null) return;

        _context.AuthorResumeItemTranslations.RemoveRange(row.AuthorResumeItemTranslations);
        _context.AuthorResumeItems.Remove(row);
        await TouchProfileAsync(_context, row.AuthorProfileID, ct);
        await _context.SaveChangesAsync(ct);
    }

    // ── Public read ─────────────────────────────────────────────────

    public async Task<AuthorPageDto?> GetPublicAsync(int websiteId, string slug, string? languageCode = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var profile = await _context.AuthorProfiles.AsNoTracking()
            .Where(p => p.WebsiteID == websiteId && p.Slug == slug && p.ResumeEnabled && p.Member.Active)
            .Include(p => p.Member).ThenInclude(m => m.Avatar)
            .Include(p => p.PhotoFile)
            .Include(p => p.ResumeFile)
            .Include(p => p.AuthorProfileTranslations)
            .Include(p => p.AuthorResumeItems.Where(i => i.IsActive)).ThenInclude(i => i.AuthorResumeItemTranslations)
            .AsSplitQuery()
            .FirstOrDefaultAsync(ct);
        if (profile is null) return null;

        var now = DateTime.UtcNow;
        var postCount = await _context.Posts.CountAsync(p =>
            p.WebsiteID == websiteId && p.AuthorMemberID == profile.MemberID && p.IsActive &&
            p.Status == PostService.PublishedStatus && (p.PublishedAt == null || p.PublishedAt <= now), ct);

        var text = AuthorProjection.Localize(profile, languageCode);
        var items = profile.AuthorResumeItems
            .Select(i => ProjectItem(i, languageCode))
            .ToList();

        // Section order follows the enum; within a section the admin's sort order wins, then newest first.
        var sections = profile.AuthorResumeItems
            .GroupBy(i => i.ItemType)
            .OrderBy(g => g.Key)
            .Select(g => new ResumeSectionDto
            {
                Type = TypeName(g.Key),
                Items = g.OrderBy(i => i.SortOrder)
                    .ThenByDescending(i => i.IsCurrent)
                    .ThenByDescending(i => i.StartDate)
                    .Select(i => items.First(x => x.ItemID == i.AuthorResumeItemID))
                    .ToList(),
            })
            .ToList();

        var timeline = profile.AuthorResumeItems
            .Where(i => i.ShowInTimeline)
            .OrderByDescending(i => i.IsCurrent)
            .ThenByDescending(i => i.EndDate ?? i.StartDate)
            .ThenByDescending(i => i.StartDate)
            .ThenBy(i => i.SortOrder)
            .Select(i => items.First(x => x.ItemID == i.AuthorResumeItemID))
            .ToList();

        return new AuthorPageDto
        {
            Slug = profile.Slug,
            Name = text.Name,
            Headline = text.Headline,
            Bio = text.Bio,
            About = text.About,
            Location = profile.Location,
            PublicEmail = profile.PublicEmail,
            WebsiteUrl = profile.WebsiteUrl,
            PhotoUrl = AuthorProjection.PhotoUrl(profile, profile.Member),
            ResumeFileUrl = profile.ResumeFile is { IsDeleted: false } file ? file.CNDUrl : null,
            Skills = SplitSkills(profile.Skills),
            SocialLinks = AuthorSocialLinks.Parse(profile.SocialLinksJson),
            Sections = sections,
            Timeline = timeline,
            PostCount = postCount,
        };
    }

    // ── Helpers ─────────────────────────────────────────────────────

    private static ResumeItemDto ProjectItem(AuthorResumeItem i, string? lang)
    {
        var t = string.IsNullOrWhiteSpace(lang) ? null : i.AuthorResumeItemTranslations.FirstOrDefault(x =>
            string.Equals(x.LanguageCode.Trim(), lang, StringComparison.OrdinalIgnoreCase));
        return new ResumeItemDto
        {
            ItemID = i.AuthorResumeItemID,
            Type = TypeName(i.ItemType),
            Title = Pick(t?.Title, i.Title)!,
            Organization = Pick(t?.Organization, i.Organization),
            Location = Pick(t?.Location, i.Location),
            Description = Pick(t?.Description, i.Description),
            Url = i.Url,
            StartDate = i.StartDate,
            EndDate = i.IsCurrent ? null : i.EndDate,
            IsCurrent = i.IsCurrent,
        };
    }

    internal static string TypeName(byte type) =>
        JsonNamingPolicy.CamelCase.ConvertName(
            Enum.IsDefined(typeof(ResumeItemType), type) ? ((ResumeItemType)type).ToString() : nameof(ResumeItemType.Other));

    private static void SyncTranslations(AppDbContext _context, AuthorProfile row, IReadOnlyList<AuthorProfileTranslation> translations)
    {
        var wanted = translations
            .Where(t => !string.IsNullOrWhiteSpace(t.LanguageCode)
                        && (HasText(t.DisplayName) || HasText(t.Headline) || HasText(t.Bio) || HasText(t.About)))
            .GroupBy(t => t.LanguageCode.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var existing in row.AuthorProfileTranslations.Where(t => !wanted.ContainsKey(t.LanguageCode.Trim().ToLowerInvariant())).ToList())
            _context.AuthorProfileTranslations.Remove(existing);

        foreach (var (code, t) in wanted)
        {
            var current = row.AuthorProfileTranslations.FirstOrDefault(x => string.Equals(x.LanguageCode.Trim(), code, StringComparison.OrdinalIgnoreCase));
            if (current is null)
            {
                current = new AuthorProfileTranslation { LanguageCode = code };
                row.AuthorProfileTranslations.Add(current);
            }
            current.DisplayName = Clean(t.DisplayName);
            current.Headline = Clean(t.Headline);
            current.Bio = Clean(t.Bio);
            current.About = Clean(t.About);
        }
    }

    private static async Task<int?> OwnFileAsync(AppDbContext _context, int? fileId, int websiteId, CancellationToken ct) =>
        fileId is int id && await _context.FileRecords.AnyAsync(f => f.FileRecordID == id && f.WebsiteID == websiteId && !f.IsDeleted, ct)
            ? id
            : null;

    private static async Task TouchProfileAsync(AppDbContext _context, int profileId, CancellationToken ct)
    {
        var profile = await _context.AuthorProfiles.FirstOrDefaultAsync(p => p.AuthorProfileID == profileId, ct);
        if (profile is not null) profile.UpdatedAt = DateTime.UtcNow;
    }

    private static string? NormalizeSkills(string? skills)
    {
        var list = SplitSkills(skills);
        return list.Count == 0 ? null : string.Join(", ", list);
    }

    private static List<string> SplitSkills(string? skills) =>
        string.IsNullOrWhiteSpace(skills)
            ? new()
            : skills.Split([',', '،', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

    private static bool HasText(string? s) => !string.IsNullOrWhiteSpace(s);

    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static string? Pick(string? translated, string? fallback) =>
        string.IsNullOrWhiteSpace(translated) ? fallback : translated;
}

/// <summary>Localized author text and photo shared by the author page and the author box under posts.</summary>
internal static class AuthorProjection
{
    public readonly record struct Text(string Name, string? Headline, string? Bio, string? About);

    public static Text Localize(AuthorProfile p, string? lang)
    {
        var t = string.IsNullOrWhiteSpace(lang) ? null : p.AuthorProfileTranslations.FirstOrDefault(x =>
            string.Equals(x.LanguageCode.Trim(), lang, StringComparison.OrdinalIgnoreCase));
        var baseName = !string.IsNullOrWhiteSpace(p.DisplayName) ? p.DisplayName!
            : p.Member is null ? string.Empty
            : $"{p.Member.Givenname} {p.Member.Surname}".Trim();
        return new Text(
            Pick(t?.DisplayName, baseName)!,
            Pick(t?.Headline, p.Headline),
            Pick(t?.Bio, p.Bio),
            Pick(t?.About, p.About));
    }

    public static string? PhotoUrl(AuthorProfile? p, Member member)
    {
        var file = p?.PhotoFile is { IsDeleted: false } photo ? photo
            : member.Avatar is { IsDeleted: false } avatar ? avatar
            : null;
        return file?.ThumbnailCDN ?? file?.CNDUrl;
    }

    /// <summary>The author box for a post's author (null when the post has no author).</summary>
    public static AuthorCardDto? Card(Member? member, string? lang)
    {
        if (member is null) return null;
        var p = member.AuthorProfile;
        if (p is null)
            return new AuthorCardDto
            {
                Name = $"{member.Givenname} {member.Surname}".Trim(),
                PhotoUrl = PhotoUrl(null, member),
            };

        var text = Localize(p, lang);
        return new AuthorCardDto
        {
            Name = text.Name,
            Slug = p.ResumeEnabled && member.Active ? p.Slug : null,
            Headline = text.Headline,
            Bio = p.ShowBioOnPosts ? text.Bio : null,
            PhotoUrl = PhotoUrl(p, member),
        };
    }

    private static string? Pick(string? translated, string? fallback) =>
        string.IsNullOrWhiteSpace(translated) ? fallback : translated;
}
