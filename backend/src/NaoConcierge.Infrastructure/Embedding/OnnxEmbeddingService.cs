using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NaoConcierge.Application.Interfaces;

namespace NaoConcierge.Infrastructure.Embedding;

public class OnnxEmbeddingService : IEmbeddingService
{
    private readonly ILogger<OnnxEmbeddingService> _logger;
    private readonly InferenceSession? _session;
    private readonly bool _useMock;

    private const string ModelPath = "models/all-MiniLM-L6-v2.onnx";

    public OnnxEmbeddingService(ILogger<OnnxEmbeddingService> logger)
    {
        _logger = logger;

        if (File.Exists(ModelPath))
        {
            _session = new InferenceSession(ModelPath);
            _useMock = false;
            _logger.LogInformation("ONNX model loaded from {ModelPath}", ModelPath);
        }
        else
        {
            _session = null;
            _useMock = true;
            _logger.LogWarning("ONNX model not found at {ModelPath}. Falling back to mock embedding generation.", ModelPath);
        }
    }

    public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        if (_useMock || _session is null)
        {
            return Task.FromResult(MockEmbeddingGenerator.Generate(text));
        }

        var tokenIds = SimpleTokenize(text);
        var inputIds = new DenseTensor<long>(tokenIds.Select(t => (long)t).ToArray(), new[] { 1, tokenIds.Length });
        var attentionMask = new DenseTensor<long>(Enumerable.Repeat(1L, tokenIds.Length).ToArray(), new[] { 1, tokenIds.Length });
        var tokenTypeIds = new DenseTensor<long>(new long[tokenIds.Length], new[] { 1, tokenIds.Length });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask),
            NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds)
        };

        using var results = _session.Run(inputs);
        var output = results.First().AsTensor<float>();
        var embedding = MeanPool(output, tokenIds.Length);

        return Task.FromResult(embedding);
    }

    public async Task<float[][]> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken ct = default)
    {
        var results = new List<float[]>();
        foreach (var text in texts)
        {
            results.Add(await GenerateEmbeddingAsync(text, ct));
        }
        return results.ToArray();
    }

    private static int[] SimpleTokenize(string text)
    {
        var words = text.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var tokens = new List<int> { 101 };
        foreach (var word in words)
        {
            tokens.Add(Math.Abs(word.GetHashCode()) % 30000 + 1000);
        }
        tokens.Add(102);
        return tokens.ToArray();
    }

    private static float[] MeanPool(Tensor<float> output, int tokenCount)
    {
        var dimensions = 384;
        var pooled = new float[dimensions];

        for (var d = 0; d < dimensions; d++)
        {
            float sum = 0;
            for (var t = 0; t < tokenCount; t++)
            {
                sum += output[0, t, d];
            }
            pooled[d] = sum / tokenCount;
        }

        var magnitude = MathF.Sqrt(pooled.Sum(v => v * v));
        if (magnitude > 0)
        {
            for (var i = 0; i < dimensions; i++)
            {
                pooled[i] /= magnitude;
            }
        }

        return pooled;
    }
}
