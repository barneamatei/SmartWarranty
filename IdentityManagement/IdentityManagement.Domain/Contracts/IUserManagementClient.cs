using IdentityManagement.Domain.DTOs;

namespace IdentityManagement.Domain.Contracts;

public interface IUserManagementClient
{
    Task CreateUserAsync(CreateUserProfileRequestDto request, CancellationToken cancellationToken = default);
}
