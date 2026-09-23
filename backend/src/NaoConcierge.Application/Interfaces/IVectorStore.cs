using NaoConcierge.Application.DTOs;
using NaoConcierge.Domain.Entities;

namespace NaoConcierge.Application.Interfaces;

public interface IVectorStore
{
    Task IndexDocumentAsync(EmbeddingDocument document, CancellationToken ct = default);
    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(float[] embedding, int topK = 5, CancellationToken ct = default);
}
