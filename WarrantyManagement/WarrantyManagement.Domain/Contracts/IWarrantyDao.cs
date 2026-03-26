using WarrantyManagement.Domain.Entities;

namespace WarrantyManagement.Domain.Contracts;

public interface IWarrantyDao
{
    Task<Warranty?> GetByIdAsync(Guid warrantyId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Warranty>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<Warranty>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Warranty>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<Warranty> AddAsync(Warranty warranty, CancellationToken cancellationToken = default);

    Task<Warranty> UpdateAsync(Warranty warranty, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid warrantyId, CancellationToken cancellationToken = default);
}
