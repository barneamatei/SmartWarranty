using NotificationManagement.Domain.Contracts;
using NotificationManagement.Domain.DTOs;
using NotificationManagement.Domain.Entities;
using NotificationManagement.Service.Exceptions;

namespace NotificationManagement.Service.Services;

public class NotificationService
{
    private readonly INotificationDao _notificationDao;

    public NotificationService(INotificationDao notificationDao)
    {
        _notificationDao = notificationDao ?? throw new ArgumentNullException(nameof(notificationDao));
    }

    public async Task<NotificationResponseDto> CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.UserId == Guid.Empty)
            throw new DomainException("UserId is invalid.");
        if (!Enum.TryParse<NotificationType>(dto.Type, true, out var type))
            throw new DomainException("Notification type is invalid.");
        if (!Enum.TryParse<NotificationChannel>(dto.Channel, true, out var channel))
            throw new DomainException("Notification channel is invalid.");

        var notification = new Notification(
            Guid.NewGuid(),
            dto.UserId,
            dto.Title,
            dto.Message,
            type,
            channel,
            dto.Metadata);

        var savedNotification = await _notificationDao.AddAsync(notification, cancellationToken);
        return MapToResponse(savedNotification);
    }

    public async Task<NotificationResponseDto?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationDao.GetByIdAsync(notificationId, cancellationToken);
        return notification == null ? null : MapToResponse(notification);
    }

    public async Task<IEnumerable<NotificationResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationDao.GetAllAsync(cancellationToken);
        return notifications.Select(MapToResponse);
    }

    public async Task<IEnumerable<NotificationResponseDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId is invalid.");

        var notifications = await _notificationDao.GetByUserIdAsync(userId, cancellationToken);
        return notifications.Select(MapToResponse);
    }

    public async Task<IEnumerable<NotificationResponseDto>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId is invalid.");

        var notifications = await _notificationDao.GetUnreadByUserIdAsync(userId, cancellationToken);
        return notifications.Select(MapToResponse);
    }

    public async Task<NotificationResponseDto> MarkSentAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await GetRequiredNotificationAsync(notificationId, cancellationToken);
        notification.MarkSent();
        var updated = await _notificationDao.UpdateAsync(notification, cancellationToken);
        return MapToResponse(updated);
    }

    public async Task<NotificationResponseDto> MarkReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await GetRequiredNotificationAsync(notificationId, cancellationToken);
        notification.MarkRead();
        var updated = await _notificationDao.UpdateAsync(notification, cancellationToken);
        return MapToResponse(updated);
    }

    public async Task<NotificationResponseDto> MarkFailedAsync(Guid notificationId, MarkNotificationFailedDto dto, CancellationToken cancellationToken = default)
    {
        var notification = await GetRequiredNotificationAsync(notificationId, cancellationToken);
        notification.MarkFailed(dto.ErrorMessage);
        var updated = await _notificationDao.UpdateAsync(notification, cancellationToken);
        return MapToResponse(updated);
    }

    public Task<bool> DeleteAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        return _notificationDao.DeleteAsync(notificationId, cancellationToken);
    }

    private async Task<Notification> GetRequiredNotificationAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        var notification = await _notificationDao.GetByIdAsync(notificationId, cancellationToken);
        if (notification == null)
            throw new DomainException($"Notification with ID {notificationId} not found.");

        return notification;
    }

    private static NotificationResponseDto MapToResponse(Notification notification)
    {
        return new NotificationResponseDto
        {
            NotificationId = notification.NotificationId,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type.ToString(),
            Channel = notification.Channel.ToString(),
            Status = notification.Status.ToString(),
            CreatedAt = notification.CreatedAt,
            SentAt = notification.SentAt,
            ReadAt = notification.ReadAt,
            Metadata = notification.Metadata,
            ErrorMessage = notification.ErrorMessage
        };
    }
}
