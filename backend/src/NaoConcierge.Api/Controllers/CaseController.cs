using Microsoft.AspNetCore.Mvc;
using NaoConcierge.Application.DTOs;
using NaoConcierge.Application.UseCases;

namespace NaoConcierge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CaseController : ControllerBase
{
    private readonly StartIntakeUseCase _startIntake;
    private readonly GetCaseStatusUseCase _getCaseStatus;
    private readonly ConfirmAttributeUseCase _confirmAttribute;

    public CaseController(
        StartIntakeUseCase startIntake,
        GetCaseStatusUseCase getCaseStatus,
        ConfirmAttributeUseCase confirmAttribute)
    {
        _startIntake = startIntake;
        _getCaseStatus = getCaseStatus;
        _confirmAttribute = confirmAttribute;
    }

    [HttpPost]
    public async Task<ActionResult<IntakeCaseDto>> Create([FromBody] CreateCaseRequestDto request, CancellationToken ct)
    {
        var result = await _startIntake.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<IntakeCaseDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _getCaseStatus.ExecuteAsync(id, ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("{id:guid}/attributes/{key}/confirm")]
    public async Task<IActionResult> ConfirmAttribute(
        Guid id, string key, [FromBody] ConfirmAttributeRequestDto request, CancellationToken ct)
    {
        await _confirmAttribute.ExecuteAsync(id, key, request, ct);
        return NoContent();
    }
}
