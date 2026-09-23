namespace NaoConcierge.Application.DTOs;

public record IntakeCaseDto(
    Guid Id,
    string RegistrationType,
    string AdvisoryProgram,
    string Purpose,
    string Status,
    ApplicantDto? Applicant,
    List<CaseAttributeDto> Attributes,
    DateTime CreatedAt,
    DateTime UpdatedAt);
