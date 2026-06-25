using IdentityManagement.Domain.DTOs;
using IdentityManagement.Domain.Entities;

namespace IdentityManagement.Domain.Contracts;

public interface IJwtTokenGenerator
{
    Task<TokenResultDto> GenerateAccessTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default);

    string GenerateRefreshToken();
}
