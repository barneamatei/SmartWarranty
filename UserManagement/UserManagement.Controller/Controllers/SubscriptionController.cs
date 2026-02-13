using Microsoft.AspNetCore.Mvc;
using UserManagement.Service.DTOs;
using UserManagement.Service.Exceptions;
using UserManagement.Service.Services;

namespace UserManagement.Controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    }

    [HttpPost]
    [ProducesResponseType(typeof(SubscriptionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SubscriptionResponseDto>> Create([FromBody] CreateSubscriptionDto dto, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = subscription.SubscriptionId }, subscription);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SubscriptionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubscriptionResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionService.GetByIdAsync(id, cancellationToken);
        if (subscription == null)
            return NotFound(new { error = $"Subscription with ID {id} not found." });
        return Ok(subscription);
    }

    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(SubscriptionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubscriptionResponseDto>> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionService.GetByUserIdAsync(userId, cancellationToken);
        if (subscription == null)
            return NotFound(new { error = $"No subscription found for user {userId}." });
        return Ok(subscription);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SubscriptionResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SubscriptionResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionService.GetAllAsync(cancellationToken);
        return Ok(subscriptions);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SubscriptionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubscriptionResponseDto>> Update(Guid id, [FromBody] UpdateSubscriptionDto dto, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionService.UpdateAsync(id, dto, cancellationToken);
        return Ok(subscription);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _subscriptionService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(new { error = $"Subscription with ID {id} not found." });
        return NoContent();
    }
}
