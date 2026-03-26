using WarrantyManagement.Domain.Entities;

namespace WarrantyManagement.Domain.Contracts;

public interface IClaimDao
{
    Task<Claim?> GetByIdAsync(Guid claimId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Claim>> GetByWarrantyIdAsync(Guid warrantyId, CancellationToken cancellationToken = default);

    Task<Claim> AddAsync(Claim claim, CancellationToken cancellationToken = default);

    Task<Claim> UpdateAsync(Claim claim, CancellationToken cancellationToken = default);
}
