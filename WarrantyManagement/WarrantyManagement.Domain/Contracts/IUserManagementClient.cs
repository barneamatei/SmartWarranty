namespace WarrantyManagement.Domain.Contracts;

public interface IUserManagementClient
{
    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default);
}
