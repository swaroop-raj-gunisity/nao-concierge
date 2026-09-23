namespace NaoConcierge.Infrastructure.Embedding;

public static class MockEmbeddingGenerator
{
    public static float[] Generate(string text, int dimensions = 384)
    {
        var seed = text.GetHashCode();
        var random = new Random(seed);
        var vector = new float[dimensions];

        for (var i = 0; i < dimensions; i++)
        {
            vector[i] = (float)(random.NextDouble() * 2 - 1);
        }

        var magnitude = MathF.Sqrt(vector.Sum(v => v * v));
        if (magnitude > 0)
        {
            for (var i = 0; i < dimensions; i++)
            {
                vector[i] /= magnitude;
            }
        }

        return vector;
    }
}
