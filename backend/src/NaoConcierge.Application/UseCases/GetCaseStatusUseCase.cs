using NaoConcierge.Application.DTOs;
using NaoConcierge.Application.Interfaces;

namespace NaoConcierge.Application.UseCases;

public class GetCaseStatusUseCase
{
    private readonly ICaseRepository _caseRepository;

    public GetCaseStatusUseCase(ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository;
    }

    public async Task<IntakeCaseDto?> ExecuteAsync(Guid caseId, CancellationToken ct = default)
    {
        var intakeCase = await _caseRepository.GetByIdAsync(caseId, ct);

        if (intakeCase is null)
            return null;

        return CaseMapper.ToDto(intakeCase);
    }
}
