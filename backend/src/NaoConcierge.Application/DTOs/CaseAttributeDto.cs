namespace NaoConcierge.Application.DTOs;

public record CaseAttributeDto(
    string AttributeKey,
    string? Value,
    double? Confidence,
    string State,
    string? Source,
    DateTime ExtractedAt);
