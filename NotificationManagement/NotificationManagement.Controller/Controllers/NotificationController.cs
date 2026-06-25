using Microsoft.AspNetCore.Mvc;
using NotificationManagement.Domain.DTOs;
using NotificationManagement.Service.Services;

namespace NotificationManagement.Controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationController(NotificationService notificationService)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    [HttpPost]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationResponseDto>> Create([FromBody] CreateNotificationDto dto, CancellationToken cancellationToken)
    {
        var notification = await _notificationService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = notification.NotificationId }, notification);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var notification = await _notificationService.GetByIdAsync(id, cancellationToken);
        if (notification == null)
            return NotFound(new { error = $"Notification with ID {id} not found." });

        return Ok(notification);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var notifications = await _notificationService.GetAllAsync(cancellationToken);
        return Ok(notifications);
    }

    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var notifications = await _notificationService.GetByUserIdAsync(userId, cancellationToken);
        return Ok(notifications);
    }

    [HttpGet("user/{userId}/unread")]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetUnreadByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var notifications = await _notificationService.GetUnreadByUserIdAsync(userId, cancellationToken);
        return Ok(notifications);
    }

    [HttpPost("{id}/mark-sent")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponseDto>> MarkSent(Guid id, CancellationToken cancellationToken)
    {
        var notification = await _notificationService.MarkSentAsync(id, cancellationToken);
        return Ok(notification);
    }

    [HttpPost("{id}/mark-read")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponseDto>> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var notification = await _notificationService.MarkReadAsync(id, cancellationToken);
        return Ok(notification);
    }

    [HttpPost("{id}/mark-failed")]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResponseDto>> MarkFailed(Guid id, [FromBody] MarkNotificationFailedDto dto, CancellationToken cancellationToken)
    {
        var notification = await _notificationService.MarkFailedAsync(id, dto, cancellationToken);
        return Ok(notification);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _notificationService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound(new { error = $"Notification with ID {id} not found." });

        return NoContent();
    }
}
