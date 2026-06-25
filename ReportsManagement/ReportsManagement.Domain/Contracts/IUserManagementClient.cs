using ReportsManagement.Domain.DTOs;

namespace ReportsManagement.Domain.Contracts;

public interface IUserManagementClient
{
    Task<IReadOnlyList<UserReportDto>> GetUsersAsync(CancellationToken cancellationToken = default);
}
