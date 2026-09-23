using NaoConcierge.Domain.ValueObjects;

namespace NaoConcierge.Domain.Entities;

public class TrustedContact
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public string Name { get; set; } = null!;
    public string Relationship { get; set; } = null!;
    public PhoneNumber Phone { get; set; } = null!;
    public EmailAddress? Email { get; set; }
    public bool Declined { get; set; }
}
