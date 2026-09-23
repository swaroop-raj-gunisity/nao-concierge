using NaoConcierge.Domain.Enums;

namespace NaoConcierge.Domain.Entities;

public class ConversationTurn
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public ConversationRole Role { get; set; }
    public string Content { get; set; } = null!;
    public DateTime Timestamp { get; set; }
}
