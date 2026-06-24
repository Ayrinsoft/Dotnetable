using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ContactMessageService : IContactMessageService
{
    private readonly AppDbContext _context;

    public ContactMessageService(AppDbContext context) => _context = context;

    public async Task<PagedResult<ContactUsMessage>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.ContactUsMessages.AsNoTracking();

        if (websiteId.HasValue)
            q = q.Where(m => m.WebsiteID == websiteId.Value);

        if (query.GetSearch(nameof(ContactUsMessage.SenderName)) is string name)
            q = q.Where(m => m.SenderName.Contains(name));
        if (query.GetSearch(nameof(ContactUsMessage.EmailAddress)) is string email)
            q = q.Where(m => m.EmailAddress.Contains(email));
        if (query.GetSearch(nameof(ContactUsMessage.MessageSubject)) is string subject)
            q = q.Where(m => m.MessageSubject.Contains(subject));
        if (query.GetSearch(nameof(ContactUsMessage.Archive)) is string archive && bool.TryParse(archive, out var isArchived))
            q = q.Where(m => m.Archive == isArchived);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(ContactUsMessage.LogTime), fallbackDescending: true)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<ContactUsMessage> { Items = items, TotalCount = total };
    }

    public async Task<ContactUsMessage?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.ContactUsMessages.FindAsync([id], ct);

    public async Task SetArchiveAsync(int id, bool archive, CancellationToken ct = default) =>
        await _context.ContactUsMessages.Where(m => m.ContactUsMessagesID == id)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.Archive, archive), ct);

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.ContactUsMessages.FindAsync([id], ct);
        if (entity is null) return;
        _context.ContactUsMessages.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
