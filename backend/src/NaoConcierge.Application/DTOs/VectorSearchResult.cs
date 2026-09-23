namespace NaoConcierge.Application.DTOs;

public record VectorSearchResult(
    string Content,
    string Source,
    double Score);
