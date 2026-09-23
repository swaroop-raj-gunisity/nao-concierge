using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using NaoConcierge.Agent.Prompts;
using NaoConcierge.Application.DTOs;
using NaoConcierge.Application.Interfaces;
using NaoConcierge.Domain.Enums;
using System.Runtime.CompilerServices;

namespace NaoConcierge.Agent.Orchestration;

public class ConciergeChatOrchestrator : IChatOrchestrator
{
    private readonly Kernel _kernel;
    private readonly ICaseRepository _caseRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly ILogger<ConciergeChatOrchestrator> _logger;

    public ConciergeChatOrchestrator(
        Kernel kernel,
        ICaseRepository caseRepository,
        IConversationRepository conversationRepository,
        ILogger<ConciergeChatOrchestrator> logger)
    {
        _kernel = kernel;
        _caseRepository = caseRepository;
        _conversationRepository = conversationRepository;
        _logger = logger;
    }

    public async Task<ChatResponse> ProcessMessageAsync(Guid caseId, string userMessage, CancellationToken ct = default)
    {
        var intakeCase = await _caseRepository.GetByIdAsync(caseId, ct);
        if (intakeCase == null)
            throw new InvalidOperationException($"Case {caseId} not found.");

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(PromptTemplates.SystemPrompt);

        var previousTurns = await _conversationRepository.GetByCaseIdAsync(caseId, ct);
        foreach (var turn in previousTurns)
        {
            switch (turn.Role)
            {
                case ConversationRole.User:
                    chatHistory.AddUserMessage(turn.Content);
                    break;
                case ConversationRole.Assistant:
                    chatHistory.AddAssistantMessage(turn.Content);
                    break;
            }
        }

        chatHistory.AddUserMessage(userMessage);

        var chatService = _kernel.GetRequiredService<IChatCompletionService>();
        var response = await chatService.GetChatMessageContentAsync(chatHistory, cancellationToken: ct);
        var reply = response.Content ?? string.Empty;

        var extractedAttributes = ExtractAttributesFromResponse(reply);

        return new ChatResponse(
            Reply: reply,
            ExtractedAttributes: extractedAttributes,
            RequiresClarification: extractedAttributes.Any(a => a.Confidence.HasValue && a.Confidence.Value < 0.70));
    }

    public async IAsyncEnumerable<string> StreamMessageAsync(
        Guid caseId, string userMessage, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var intakeCase = await _caseRepository.GetByIdAsync(caseId, ct);
        if (intakeCase == null)
            throw new InvalidOperationException($"Case {caseId} not found.");

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(PromptTemplates.SystemPrompt);

        var previousTurns = await _conversationRepository.GetByCaseIdAsync(caseId, ct);
        foreach (var turn in previousTurns)
        {
            switch (turn.Role)
            {
                case ConversationRole.User:
                    chatHistory.AddUserMessage(turn.Content);
                    break;
                case ConversationRole.Assistant:
                    chatHistory.AddAssistantMessage(turn.Content);
                    break;
            }
        }

        chatHistory.AddUserMessage(userMessage);

        var chatService = _kernel.GetRequiredService<IChatCompletionService>();
        await foreach (var chunk in chatService.GetStreamingChatMessageContentsAsync(chatHistory, cancellationToken: ct))
        {
            if (chunk.Content != null)
                yield return chunk.Content;
        }
    }

    private static List<CaseAttributeDto> ExtractAttributesFromResponse(string response)
    {
        return new List<CaseAttributeDto>();
    }
}
