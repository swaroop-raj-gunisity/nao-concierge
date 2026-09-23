using NaoConcierge.Application.DTOs;
using NaoConcierge.Application.Interfaces;
using NaoConcierge.Domain.Entities;
using NaoConcierge.Domain.Enums;

namespace NaoConcierge.Application.UseCases;

public class StartIntakeUseCase
{
    private readonly ICaseRepository _caseRepository;

    public StartIntakeUseCase(ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository;
    }

    public async Task<IntakeCaseDto> ExecuteAsync(CreateCaseRequestDto request, CancellationToken ct = default)
    {
        var intakeCase = new IntakeCase
        {
            Id = Guid.NewGuid(),
            RegistrationType = Enum.Parse<RegistrationType>(request.RegistrationType, ignoreCase: true),
            AdvisoryProgram = Enum.Parse<AdvisoryProgram>(request.AdvisoryProgram, ignoreCase: true),
            Purpose = Enum.Parse<AccountPurpose>(request.Purpose, ignoreCase: true),
            Status = CaseStatus.Draft,
            Applicant = new Applicant(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _caseRepository.CreateAsync(intakeCase, ct);
        return CaseMapper.ToDto(created);
    }
}
