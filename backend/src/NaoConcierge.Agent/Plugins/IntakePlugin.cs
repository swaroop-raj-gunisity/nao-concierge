using Microsoft.SemanticKernel;
using NaoConcierge.Application.Interfaces;
using NaoConcierge.Domain.Enums;
using NaoConcierge.Domain.ValueObjects;
using System.ComponentModel;

namespace NaoConcierge.Agent.Plugins;

public class IntakePlugin
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;
    private readonly IRedactionService _redactionService;

    public IntakePlugin(
        IEmbeddingService embeddingService,
        IVectorStore vectorStore,
        IRedactionService redactionService)
    {
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _redactionService = redactionService;
    }

    [KernelFunction("search_crm_notes")]
    [Description("Search CRM notes for relevant client information using semantic search")]
    public async Task<string> SearchCrmNotesAsync(
        [Description("The search query to find relevant CRM notes")] string query,
        CancellationToken ct = default)
    {
        var embedding = await _embeddingService.GenerateEmbeddingAsync(query, ct);
        var results = await _vectorStore.SearchAsync(embedding, topK: 5, ct);

        if (results.Count == 0)
            return "No relevant CRM notes found.";

        var output = results.Select(r => $"[Score: {r.Score:F2}] {r.Source}: {r.Content}");
        return string.Join("\n\n", output);
    }

    [KernelFunction("check_confidence")]
    [Description("Check if an extracted attribute's confidence score requires clarification")]
    public string CheckConfidence(
        [Description("The confidence score between 0.0 and 1.0")] double score)
    {
        var confidence = ConfidenceScore.Create(score);
        if (confidence.RequiresClarification)
            return $"Confidence {score:F2} is below threshold (0.70). Clarification is required.";
        return $"Confidence {score:F2} meets the threshold. Attribute can be proposed.";
    }

    [KernelFunction("redact_pii")]
    [Description("Redact PII from text based on the specified sensitivity level")]
    public async Task<string> RedactPiiAsync(
        [Description("The text to redact")] string text,
        [Description("PII level: None, Indirect, Direct, or Sensitive")] string piiLevel,
        CancellationToken ct = default)
    {
        var level = Enum.Parse<PiiLevel>(piiLevel, ignoreCase: true);
        return await _redactionService.RedactAsync(text, level, ct);
    }
}
