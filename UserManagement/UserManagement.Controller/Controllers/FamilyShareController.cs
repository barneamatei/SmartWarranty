using Microsoft.AspNetCore.Mvc;
using UserManagement.Domain.DTOs;
using UserManagement.Service.Exceptions;
using UserManagement.Service.Services;

namespace UserManagement.Controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FamilyShareController : ControllerBase
{
    private readonly FamilyShareService _familyShareService;

    public FamilyShareController(FamilyShareService familyShareService)
    {
        _familyShareService = familyShareService ?? throw new ArgumentNullException(nameof(familyShareService));
    }

    [HttpPost]
    [ProducesResponseType(typeof(FamilyShareResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FamilyShareResponseDto>> Create([FromBody] CreateFamilyShareDto dto, CancellationToken cancellationToken)
    {
        var share = await _familyShareService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = share.ShareId }, share);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FamilyShareResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FamilyShareResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var share = await _familyShareService.GetByIdAsync(id, cancellationToken);
        if (share == null)
            return NotFound(new { error = $"Family share with ID {id} not found." });
        return Ok(share);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FamilyShareResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FamilyShareResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var shares = await _familyShareService.GetAllAsync(cancellationToken);
        return Ok(shares);
    }

    [HttpGet("owner/{ownerUserId}")]
    [ProducesResponseType(typeof(IEnumerable<FamilyShareResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FamilyShareResponseDto>>> GetByOwnerId(Guid ownerUserId, CancellationToken cancellationToken)
    {
        var shares = await _familyShareService.GetByOwnerIdAsync(ownerUserId, cancellationToken);
        return Ok(shares);
    }

    [HttpGet("member/{memberUserId}")]
    [ProducesResponseType(typeof(IEnumerable<FamilyShareResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FamilyShareResponseDto>>> GetByMemberId(Guid memberUserId, CancellationToken cancellationToken)
    {
        var shares = await _familyShareService.GetByMemberIdAsync(memberUserId, cancellationToken);
        return Ok(shares);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(FamilyShareResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FamilyShareResponseDto>> Update(Guid id, [FromBody] UpdateFamilyShareDto dto, CancellationToken cancellationToken)
    {
        var share = await _familyShareService.UpdateAsync(id, dto, cancellationToken);
        return Ok(share);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _familyShareService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(new { error = $"Family share with ID {id} not found." });
        return NoContent();
    }
}

