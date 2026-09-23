using NaoConcierge.Application.Interfaces;

namespace NaoConcierge.Infrastructure.Azure;

public class AzureOpenAiEmbeddingService : IEmbeddingService
{
    public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        throw new NotImplementedException("Azure OpenAI embedding service is not yet configured. Set UseAzureCloud=false for local mode.");
    }

    public Task<float[][]> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken ct = default)
    {
        throw new NotImplementedException("Azure OpenAI embedding service is not yet configured. Set UseAzureCloud=false for local mode.");
    }
}
