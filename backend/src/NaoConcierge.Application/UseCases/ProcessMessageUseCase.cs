using NaoConcierge.Application.DTOs;
using NaoConcierge.Application.Interfaces;
using NaoConcierge.Domain.Entities;
using NaoConcierge.Domain.Enums;

namespace NaoConcierge.Application.UseCases;

public class ProcessMessageUseCase
{
    private readonly ICaseRepository _caseRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IChatOrchestrator _chatOrchestrator;

    public ProcessMessageUseCase(
        ICaseRepository caseRepository,
        IConversationRepository conversationRepository,
        IChatOrchestrator chatOrchestrator)
    {
        _caseRepository = caseRepository;
        _conversationRepository = conversationRepository;
        _chatOrchestrator = chatOrchestrator;
    }

    public async Task<ChatResponseDto> ExecuteAsync(ChatRequestDto request, CancellationToken ct = default)
    {
        var intakeCase = await _caseRepository.GetByIdAsync(request.CaseId, ct)
            ?? throw new KeyNotFoundException($"Case {request.CaseId} not found.");

        var userTurn = new ConversationTurn
        {
            Id = Guid.NewGuid(),
            CaseId = request.CaseId,
            Role = ConversationRole.User,
            Content = request.Message,
            Timestamp = DateTime.UtcNow
        };
        await _conversationRepository.AddTurnAsync(userTurn, ct);

        var chatResponse = await _chatOrchestrator.ProcessMessageAsync(request.CaseId, request.Message, ct);

        var assistantTurn = new ConversationTurn
        {
            Id = Guid.NewGuid(),
            CaseId = request.CaseId,
            Role = ConversationRole.Assistant,
            Content = chatResponse.Reply,
            Timestamp = DateTime.UtcNow
        };
        await _conversationRepository.AddTurnAsync(assistantTurn, ct);

        return new ChatResponseDto(
            chatResponse.Reply,
            chatResponse.ExtractedAttributes,
            chatResponse.RequiresClarification);
    }
}
