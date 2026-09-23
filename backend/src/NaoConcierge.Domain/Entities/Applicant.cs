using NaoConcierge.Domain.Enums;
using NaoConcierge.Domain.ValueObjects;

namespace NaoConcierge.Domain.Entities;

public class Applicant
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public LegalName LegalName { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public string? TaxId { get; set; }
    public CitizenshipStatus? CitizenshipStatus { get; set; }
    public Address? LegalAddress { get; set; }
    public EmailAddress? Email { get; set; }
    public PhoneNumber? MobilePhone { get; set; }
    public EmploymentStatus? EmploymentStatus { get; set; }
    public string? EmployerName { get; set; }
}
