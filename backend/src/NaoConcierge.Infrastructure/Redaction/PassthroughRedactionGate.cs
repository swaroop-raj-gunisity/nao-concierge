using NaoConcierge.Application.Interfaces;
using NaoConcierge.Domain.Enums;

namespace NaoConcierge.Infrastructure.Redaction;

public class PassthroughRedactionGate : IRedactionService
{
    public Task<string> RedactAsync(string text, PiiLevel level, CancellationToken ct = default)
    {
        return Task.FromResult(text);
    }
}
