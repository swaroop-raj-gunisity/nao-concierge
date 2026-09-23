using Microsoft.EntityFrameworkCore;
using NaoConcierge.Application.Interfaces;
using NaoConcierge.Domain.Entities;

namespace NaoConcierge.Infrastructure.Persistence;

public class ConversationRepository : IConversationRepository
{
    private readonly NaoConciergeDbContext _context;

    public ConversationRepository(NaoConciergeDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ConversationTurn>> GetByCaseIdAsync(Guid caseId, CancellationToken ct = default)
    {
        return await _context.ConversationTurns
            .Where(t => t.CaseId == caseId)
            .OrderBy(t => t.Timestamp)
            .ToListAsync(ct);
    }

    public async Task AddTurnAsync(ConversationTurn turn, CancellationToken ct = default)
    {
        _context.ConversationTurns.Add(turn);
        await _context.SaveChangesAsync(ct);
    }
}
