namespace NaoConcierge.Application.DTOs;

public record CreateCaseRequestDto(
    string RegistrationType,
    string AdvisoryProgram,
    string Purpose);
