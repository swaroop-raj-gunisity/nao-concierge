using NaoConcierge.Domain.Enums;
using NaoConcierge.Domain.ValueObjects;

namespace NaoConcierge.Domain.Entities;

public class CaseAttribute
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public string AttributeKey { get; set; } = null!;
    public string? RawValue { get; set; }
    public string? NormalizedValue { get; set; }
    public ConfidenceScore? Confidence { get; set; }
    public AttributeState State { get; set; }
    public string? Source { get; set; }
    public DateTime ExtractedAt { get; set; }
}
