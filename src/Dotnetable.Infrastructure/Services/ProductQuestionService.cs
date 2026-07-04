using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ProductQuestionService : IProductQuestionService
{
    private readonly AppDbContext _context;

    public ProductQuestionService(AppDbContext context) => _context = context;

    public async Task<ProductQuestion> AskAsync(int websiteId, int clientId, int productId, string body, CancellationToken ct = default)
    {
        var question = new ProductQuestion
        {
            WebsiteID = websiteId,
            ProductID = productId,
            WebsiteClientID = clientId,
            Body = body,
            Status = (byte)ModerationStatus.Pending,
            Approved = false,
            CreatedAt = DateTime.UtcNow,
        };
        _context.ProductQuestions.Add(question);
        await _context.SaveChangesAsync(ct);
        return question;
    }

    public async Task<ProductAnswer> AnswerAsync(int questionId, int? clientId, int? vendorId, string body, CancellationToken ct = default)
    {
        var answer = new ProductAnswer
        {
            ProductQuestionID = questionId,
            WebsiteClientID = clientId,
            VendorID = vendorId,
            Body = body,
            Status = (byte)ModerationStatus.Pending,
            Approved = false,
            CreatedAt = DateTime.UtcNow,
        };
        _context.ProductAnswers.Add(answer);
        await _context.SaveChangesAsync(ct);
        return answer;
    }

    public async Task<PagedResult<ProductQuestion>> GetApprovedAsync(int productId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.ProductQuestions.AsNoTracking()
            .Include(qn => qn.WebsiteClient)
            .Include(qn => qn.ProductAnswers.Where(a => a.Approved))
            .Where(qn => qn.ProductID == productId && qn.Approved);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(qn => qn.CreatedAt).Skip(query.Skip).Take(query.Take).ToListAsync(ct);
        return new PagedResult<ProductQuestion> { Items = items, TotalCount = total };
    }

    public async Task<PagedResult<ProductQuestion>> GetPendingAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.ProductQuestions.AsNoTracking()
            .Include(qn => qn.Product).Include(qn => qn.WebsiteClient).Include(qn => qn.ProductAnswers)
            .Where(qn => qn.Status == (byte)ModerationStatus.Pending || qn.ProductAnswers.Any(a => a.Status == (byte)ModerationStatus.Pending));
        if (websiteId is int wid) q = q.Where(qn => qn.WebsiteID == wid);

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(qn => qn.CreatedAt).Skip(query.Skip).Take(query.Take).ToListAsync(ct);
        return new PagedResult<ProductQuestion> { Items = items, TotalCount = total };
    }

    public async Task<bool> ModerateQuestionAsync(int questionId, bool approve, CancellationToken ct = default)
    {
        var question = await _context.ProductQuestions.FirstOrDefaultAsync(qn => qn.ProductQuestionID == questionId, ct);
        if (question is null) return false;

        question.Approved = approve;
        question.Status = (byte)(approve ? ModerationStatus.Approved : ModerationStatus.Rejected);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ModerateAnswerAsync(int answerId, bool approve, CancellationToken ct = default)
    {
        var answer = await _context.ProductAnswers.FirstOrDefaultAsync(a => a.ProductAnswerID == answerId, ct);
        if (answer is null) return false;

        answer.Approved = approve;
        answer.Status = (byte)(approve ? ModerationStatus.Approved : ModerationStatus.Rejected);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task LikeAnswerAsync(int answerId, CancellationToken ct = default) =>
        await _context.ProductAnswers.Where(a => a.ProductAnswerID == answerId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.LikeCount, a => a.LikeCount + 1), ct);
}
