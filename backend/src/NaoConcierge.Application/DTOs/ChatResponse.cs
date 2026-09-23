namespace NaoConcierge.Application.DTOs;

public record ChatResponse(
    string Reply,
    List<CaseAttributeDto> ExtractedAttributes,
    bool RequiresClarification);
