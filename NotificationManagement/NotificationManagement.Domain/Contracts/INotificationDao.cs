using NotificationManagement.Domain.Entities;

namespace NotificationManagement.Domain.Contracts;

public interface INotificationDao
{
    Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default);

    Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Notification>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Notification> UpdateAsync(Notification notification, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid notificationId, CancellationToken cancellationToken = default);
}
