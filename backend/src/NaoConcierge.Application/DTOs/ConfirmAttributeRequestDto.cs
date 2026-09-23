namespace NaoConcierge.Application.DTOs;

public record ConfirmAttributeRequestDto(
    string Action,
    string? CorrectedValue);
