namespace NaoConcierge.Application.DTOs;

public record ChatResponseDto(
    string Reply,
    List<CaseAttributeDto> ExtractedAttributes,
    bool RequiresClarification);
