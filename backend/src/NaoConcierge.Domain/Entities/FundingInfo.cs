using NaoConcierge.Domain.Enums;
using NaoConcierge.Domain.ValueObjects;

namespace NaoConcierge.Domain.Entities;

public class FundingInfo
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public FundingMethod Method { get; set; }
    public Money InitialAmount { get; set; } = null!;
    public string? DeliveringInstitutionName { get; set; }
    public string? DeliveringAccountNumber { get; set; }
    public string? DeliveringRegistrationType { get; set; }
    public string? DeliveringRegistrationId { get; set; }
    public TransferScope? TransferScope { get; set; }
}
