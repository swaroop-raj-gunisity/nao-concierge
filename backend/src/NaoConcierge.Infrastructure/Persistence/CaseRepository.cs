using Microsoft.EntityFrameworkCore;
using NaoConcierge.Application.Interfaces;
using NaoConcierge.Domain.Entities;

namespace NaoConcierge.Infrastructure.Persistence;

public class CaseRepository : ICaseRepository
{
    private readonly NaoConciergeDbContext _context;

    public CaseRepository(NaoConciergeDbContext context)
    {
        _context = context;
    }

    public async Task<IntakeCase?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.IntakeCases
            .Include(c => c.Applicant)
            .Include(c => c.Attributes)
            .Include(c => c.ConversationHistory)
            .Include(c => c.JointOwners)
            .Include(c => c.Beneficiaries)
            .Include(c => c.TrustedContact)
            .Include(c => c.FundingInfo)
            .Include(c => c.EntityInfo)
                .ThenInclude(e => e!.ResponsibleParties)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IReadOnlyList<IntakeCase>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.IntakeCases
            .Include(c => c.Attributes)
            .ToListAsync(ct);
    }

    public async Task<IntakeCase> CreateAsync(IntakeCase intakeCase, CancellationToken ct = default)
    {
        _context.IntakeCases.Add(intakeCase);
        await _context.SaveChangesAsync(ct);
        return intakeCase;
    }

    public async Task UpdateAsync(IntakeCase intakeCase, CancellationToken ct = default)
    {
        _context.IntakeCases.Update(intakeCase);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.IntakeCases.FindAsync(new object[] { id }, ct);
        if (entity is not null)
        {
            _context.IntakeCases.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }
}
