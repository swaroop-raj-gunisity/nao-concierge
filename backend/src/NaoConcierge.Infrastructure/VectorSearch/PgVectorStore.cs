using Microsoft.EntityFrameworkCore;
using NaoConcierge.Application.DTOs;
using NaoConcierge.Application.Interfaces;
using NaoConcierge.Domain.Entities;
using NaoConcierge.Infrastructure.Persistence;

namespace NaoConcierge.Infrastructure.VectorSearch;

public class PgVectorStore : IVectorStore
{
    private readonly NaoConciergeDbContext _context;

    public PgVectorStore(NaoConciergeDbContext context)
    {
        _context = context;
    }

    public async Task IndexDocumentAsync(EmbeddingDocument document, CancellationToken ct = default)
    {
        _context.EmbeddingDocuments.Add(document);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(float[] embedding, int topK = 5, CancellationToken ct = default)
    {
        var vectorString = "[" + string.Join(",", embedding) + "]";

        var results = await _context.EmbeddingDocuments
            .FromSqlRaw(
                @"SELECT ""Id"", ""Content"", ""Source"", ""Embedding"", ""IndexedAt""
                  FROM ""EmbeddingDocuments""
                  ORDER BY ""Embedding"" <=> CAST({0} AS vector)
                  LIMIT {1}",
                vectorString, topK)
            .Select(d => new { d.Content, d.Source, d.Embedding })
            .ToListAsync(ct);

        return results.Select(r => new VectorSearchResult(
            r.Content,
            r.Source,
            1.0 - CosineDistance(embedding, r.Embedding)
        )).ToList();
    }

    private static double CosineDistance(float[] a, float[] b)
    {
        double dot = 0, magA = 0, magB = 0;
        for (var i = 0; i < a.Length && i < b.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        var magnitude = Math.Sqrt(magA) * Math.Sqrt(magB);
        return magnitude > 0 ? 1.0 - (dot / magnitude) : 1.0;
    }
}
