using NaoConcierge.Domain.Entities;

namespace NaoConcierge.Application.Interfaces;

public interface ICaseRepository
{
    Task<IntakeCase?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<IntakeCase>> GetAllAsync(CancellationToken ct = default);
    Task<IntakeCase> CreateAsync(IntakeCase intakeCase, CancellationToken ct = default);
    Task UpdateAsync(IntakeCase intakeCase, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
