using NaoConcierge.Domain.Enums;

namespace NaoConcierge.Domain.Entities;

public class IntakeCase
{
    public Guid Id { get; set; }
    public RegistrationType RegistrationType { get; set; }
    public AdvisoryProgram AdvisoryProgram { get; set; }
    public AccountPurpose Purpose { get; set; }
    public CaseStatus Status { get; set; }

    public Applicant Applicant { get; set; } = null!;
    public List<CaseAttribute> Attributes { get; set; } = new();
    public List<ConversationTurn> ConversationHistory { get; set; } = new();

    public List<JointOwner>? JointOwners { get; set; }
    public EntityInfo? EntityInfo { get; set; }
    public List<Beneficiary>? Beneficiaries { get; set; }

    public TrustedContact? TrustedContact { get; set; }
    public FundingInfo? FundingInfo { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
