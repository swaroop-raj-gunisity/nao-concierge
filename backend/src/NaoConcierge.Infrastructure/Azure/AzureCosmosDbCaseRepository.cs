using NaoConcierge.Application.Interfaces;
using NaoConcierge.Domain.Entities;

namespace NaoConcierge.Infrastructure.Azure;

public class AzureCosmosDbCaseRepository : ICaseRepository
{
    public Task<IntakeCase?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException("Azure Cosmos DB is not yet configured. Set UseAzureCloud=false for local mode.");
    }

    public Task<IReadOnlyList<IntakeCase>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException("Azure Cosmos DB is not yet configured. Set UseAzureCloud=false for local mode.");
    }

    public Task<IntakeCase> CreateAsync(IntakeCase intakeCase, CancellationToken ct = default)
    {
        throw new NotImplementedException("Azure Cosmos DB is not yet configured. Set UseAzureCloud=false for local mode.");
    }

    public Task UpdateAsync(IntakeCase intakeCase, CancellationToken ct = default)
    {
        throw new NotImplementedException("Azure Cosmos DB is not yet configured. Set UseAzureCloud=false for local mode.");
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException("Azure Cosmos DB is not yet configured. Set UseAzureCloud=false for local mode.");
    }
}
