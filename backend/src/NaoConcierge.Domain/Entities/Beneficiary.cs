using NaoConcierge.Domain.Enums;
using NaoConcierge.Domain.ValueObjects;

namespace NaoConcierge.Domain.Entities;

public class Beneficiary
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public LegalName LegalName { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public string? Relationship { get; set; }
    public BeneficiaryClass Class { get; set; }
    public decimal SharePercentage { get; set; }
}
