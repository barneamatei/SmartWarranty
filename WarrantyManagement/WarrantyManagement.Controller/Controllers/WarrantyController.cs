using Microsoft.AspNetCore.Mvc;
using WarrantyManagement.Domain.DTOs;
using WarrantyManagement.Service.Services;

namespace WarrantyManagement.Controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarrantyController : ControllerBase
{
    private readonly IWarrantyService _warrantyService;

    public WarrantyController(IWarrantyService warrantyService)
    {
        _warrantyService = warrantyService ?? throw new ArgumentNullException(nameof(warrantyService));
    }

    [HttpPost]
    [ProducesResponseType(typeof(WarrantyResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WarrantyResponseDto>> Create([FromBody] CreateWarrantyDto dto, CancellationToken cancellationToken)
    {
        var warranty = await _warrantyService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = warranty.WarrantyId }, warranty);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(WarrantyResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WarrantyResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var warranty = await _warrantyService.GetByIdAsync(id, cancellationToken);
        if (warranty == null)
            return NotFound(new { error = $"Warranty with ID {id} not found." });

        return Ok(warranty);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WarrantyResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WarrantyResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var warranties = await _warrantyService.GetAllAsync(cancellationToken);
        return Ok(warranties);
    }

    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<WarrantyResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WarrantyResponseDto>>> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var warranties = await _warrantyService.GetByUserIdAsync(userId, cancellationToken);
        return Ok(warranties);
    }

    [HttpGet("product/{productId}")]
    [ProducesResponseType(typeof(IEnumerable<WarrantyResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WarrantyResponseDto>>> GetByProductId(Guid productId, CancellationToken cancellationToken)
    {
        var warranties = await _warrantyService.GetByProductIdAsync(productId, cancellationToken);
        return Ok(warranties);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(WarrantyResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WarrantyResponseDto>> Update(Guid id, [FromBody] UpdateWarrantyDto dto, CancellationToken cancellationToken)
    {
        var warranty = await _warrantyService.UpdateAsync(id, dto, cancellationToken);
        return Ok(warranty);
    }

    [HttpPost("{id}/refresh-status")]
    [ProducesResponseType(typeof(WarrantyResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WarrantyResponseDto>> RefreshStatus(Guid id, CancellationToken cancellationToken)
    {
        var warranty = await _warrantyService.RefreshStatusAsync(id, cancellationToken);
        return Ok(warranty);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _warrantyService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(new { error = $"Warranty with ID {id} not found." });

        return NoContent();
    }
}
