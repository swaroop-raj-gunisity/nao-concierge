using NaoConcierge.Application.DTOs;
using NaoConcierge.Domain.Entities;

namespace NaoConcierge.Application.UseCases;

internal static class CaseMapper
{
    public static IntakeCaseDto ToDto(IntakeCase entity)
    {
        return new IntakeCaseDto(
            entity.Id,
            entity.RegistrationType.ToString(),
            entity.AdvisoryProgram.ToString(),
            entity.Purpose.ToString(),
            entity.Status.ToString(),
            entity.Applicant is not null ? ToDto(entity.Applicant) : null,
            entity.Attributes.Select(ToDto).ToList(),
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    public static CaseAttributeDto ToDto(CaseAttribute attr)
    {
        return new CaseAttributeDto(
            attr.AttributeKey,
            attr.NormalizedValue ?? attr.RawValue,
            attr.Confidence?.Value,
            attr.State.ToString(),
            attr.Source,
            attr.ExtractedAt);
    }

    private static ApplicantDto ToDto(Applicant applicant)
    {
        return new ApplicantDto(
            applicant.LegalName?.First,
            applicant.LegalName?.Middle,
            applicant.LegalName?.Last,
            applicant.LegalName?.Suffix,
            applicant.DateOfBirth,
            applicant.CitizenshipStatus?.ToString(),
            applicant.Email?.Value,
            applicant.MobilePhone?.Value,
            applicant.EmploymentStatus?.ToString(),
            applicant.EmployerName);
    }
}
