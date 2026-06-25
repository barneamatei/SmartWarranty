namespace WarrantyManagement.Domain.Contracts;

public interface INotificationManagementClient
{
    Task CreateNotificationAsync(Guid userId, string title, string message, string type, string channel, string? metadata = null, CancellationToken cancellationToken = default);
}
