using NaoConcierge.Domain.Enums;
using NaoConcierge.Domain.ValueObjects;

namespace NaoConcierge.Domain.Entities;

public class ResponsibleParty
{
    public Guid Id { get; set; }
    public Guid EntityInfoId { get; set; }
    public LegalName LegalName { get; set; } = null!;
    public ResponsiblePartyRole Role { get; set; }
    public string? TaxId { get; set; }
}
