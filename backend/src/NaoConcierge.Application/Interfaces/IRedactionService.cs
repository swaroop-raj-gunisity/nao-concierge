using NaoConcierge.Domain.Enums;

namespace NaoConcierge.Application.Interfaces;

public interface IRedactionService
{
    Task<string> RedactAsync(string text, PiiLevel level, CancellationToken ct = default);
}
