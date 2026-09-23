using NaoConcierge.Application.DTOs;
using NaoConcierge.Application.Interfaces;
using NaoConcierge.Domain.Entities;

namespace NaoConcierge.Infrastructure.Azure;

public class AzureAiSearchVectorStore : IVectorStore
{
    public Task IndexDocumentAsync(EmbeddingDocument document, CancellationToken ct = default)
    {
        throw new NotImplementedException("Azure AI Search is not yet configured. Set UseAzureCloud=false for local mode.");
    }

    public Task<IReadOnlyList<VectorSearchResult>> SearchAsync(float[] embedding, int topK = 5, CancellationToken ct = default)
    {
        throw new NotImplementedException("Azure AI Search is not yet configured. Set UseAzureCloud=false for local mode.");
    }
}
