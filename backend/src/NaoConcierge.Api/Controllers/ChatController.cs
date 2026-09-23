using Microsoft.AspNetCore.Mvc;
using NaoConcierge.Application.DTOs;
using NaoConcierge.Application.UseCases;

namespace NaoConcierge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ProcessMessageUseCase _processMessage;

    public ChatController(ProcessMessageUseCase processMessage)
    {
        _processMessage = processMessage;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponseDto>> Post([FromBody] ChatRequestDto request, CancellationToken ct)
    {
        var response = await _processMessage.ExecuteAsync(request, ct);
        return Ok(response);
    }

    [HttpGet("stream")]
    public async Task Stream([FromQuery] Guid caseId, [FromQuery] string message, CancellationToken ct)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        var orchestrator = HttpContext.RequestServices.GetRequiredService<NaoConcierge.Application.Interfaces.IChatOrchestrator>();

        await foreach (var chunk in orchestrator.StreamMessageAsync(caseId, message, ct))
        {
            await Response.WriteAsync($"data: {chunk}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }

        await Response.WriteAsync("data: [DONE]\n\n", ct);
        await Response.Body.FlushAsync(ct);
    }
}
