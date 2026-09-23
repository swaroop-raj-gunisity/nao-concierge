using NaoConcierge.Domain.Entities;

namespace NaoConcierge.Application.Interfaces;

public interface IConversationRepository
{
    Task<IReadOnlyList<ConversationTurn>> GetByCaseIdAsync(Guid caseId, CancellationToken ct = default);
    Task AddTurnAsync(ConversationTurn turn, CancellationToken ct = default);
}
