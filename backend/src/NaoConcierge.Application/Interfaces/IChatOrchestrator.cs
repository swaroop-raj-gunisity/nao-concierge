using NaoConcierge.Application.DTOs;

namespace NaoConcierge.Application.Interfaces;

public interface IChatOrchestrator
{
    Task<ChatResponse> ProcessMessageAsync(Guid caseId, string userMessage, CancellationToken ct = default);
    IAsyncEnumerable<string> StreamMessageAsync(Guid caseId, string userMessage, CancellationToken ct = default);
}
