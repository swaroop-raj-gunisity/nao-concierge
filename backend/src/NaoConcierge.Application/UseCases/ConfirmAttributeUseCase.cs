using NaoConcierge.Application.DTOs;
using NaoConcierge.Application.Interfaces;
using NaoConcierge.Domain.Entities;
using NaoConcierge.Domain.Enums;

namespace NaoConcierge.Application.UseCases;

public class ConfirmAttributeUseCase
{
    private readonly ICaseRepository _caseRepository;

    public ConfirmAttributeUseCase(ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository;
    }

    public async Task ExecuteAsync(Guid caseId, string attributeKey, ConfirmAttributeRequestDto request, CancellationToken ct = default)
    {
        var intakeCase = await _caseRepository.GetByIdAsync(caseId, ct)
            ?? throw new KeyNotFoundException($"Case {caseId} not found.");

        var attribute = intakeCase.Attributes.FirstOrDefault(a => a.AttributeKey == attributeKey)
            ?? throw new KeyNotFoundException($"Attribute '{attributeKey}' not found on case {caseId}.");

        var targetState = request.Action.ToLowerInvariant() switch
        {
            "confirm" => AttributeState.ExtractedConfirmed,
            "reject" => AttributeState.Rejected,
            _ => throw new ArgumentException($"Invalid action '{request.Action}'. Must be 'confirm' or 'reject'.")
        };

        AttributeStateMachine.ValidateTransition(attribute.State, targetState);

        attribute.State = targetState;

        if (request.CorrectedValue is not null)
        {
            attribute.NormalizedValue = request.CorrectedValue;
        }

        intakeCase.UpdatedAt = DateTime.UtcNow;
        await _caseRepository.UpdateAsync(intakeCase, ct);
    }
}
