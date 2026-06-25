using IdentityManagement.Domain.Entities;

namespace IdentityManagement.Domain.Contracts;

public interface IRefreshTokenDao
{
    Task<RefreshToken> AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
}
