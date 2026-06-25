using Microsoft.AspNetCore.Mvc;
using WarrantyManagement.Domain.DTOs;
using WarrantyManagement.Service.Services;

namespace WarrantyManagement.Controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClaimController : ControllerBase
{
    private readonly ClaimService _claimService;

    public ClaimController(ClaimService claimService)
    {
        _claimService = claimService ?? throw new ArgumentNullException(nameof(claimService));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClaimResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClaimResponseDto>> Create([FromBody] CreateClaimDto dto, CancellationToken cancellationToken)
    {
        var claim = await _claimService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = claim.ClaimId }, claim);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClaimResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var claim = await _claimService.GetByIdAsync(id, cancellationToken);
        if (claim == null)
            return NotFound(new { error = $"Claim with ID {id} not found." });

        return Ok(claim);
    }

    [HttpGet("warranty/{warrantyId}")]
    [ProducesResponseType(typeof(IEnumerable<ClaimResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClaimResponseDto>>> GetByWarrantyId(Guid warrantyId, CancellationToken cancellationToken)
    {
        var claims = await _claimService.GetByWarrantyIdAsync(warrantyId, cancellationToken);
        return Ok(claims);
    }

    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ClaimResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimResponseDto>> UpdateStatus(Guid id, [FromBody] UpdateClaimStatusDto dto, CancellationToken cancellationToken)
    {
        var claim = await _claimService.UpdateStatusAsync(id, dto, cancellationToken);
        return Ok(claim);
    }

    [HttpPost("{id}/close")]
    [ProducesResponseType(typeof(ClaimResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimResponseDto>> Close(Guid id, CancellationToken cancellationToken)
    {
        var claim = await _claimService.CloseAsync(id, cancellationToken);
        return Ok(claim);
    }
}
