namespace NaoConcierge.Domain.Entities;

public class EntityInfo
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public string LegalTitle { get; set; } = null!;
    public List<ResponsibleParty> ResponsibleParties { get; set; } = new();
}
