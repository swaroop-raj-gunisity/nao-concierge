namespace NaoConcierge.Domain.Entities;

public class EmbeddingDocument
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public string Source { get; set; } = null!;
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public DateTime IndexedAt { get; set; }
}
