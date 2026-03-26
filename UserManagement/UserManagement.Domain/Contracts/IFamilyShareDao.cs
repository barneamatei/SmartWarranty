using UserManagement.Domain.Entities;

namespace UserManagement.Domain.Contracts;

public interface IFamilyShareDao
{
    Task<FamilyShare?> GetByIdAsync(Guid shareId, CancellationToken cancellationToken = default);

    Task<IEnumerable<FamilyShare>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<FamilyShare>> GetByOwnerIdAsync(Guid ownerUserId, CancellationToken cancellationToken = default);

    Task<IEnumerable<FamilyShare>> GetByMemberIdAsync(Guid memberUserId, CancellationToken cancellationToken = default);

    Task<FamilyShare?> GetByOwnerAndMemberAsync(Guid ownerUserId, Guid memberUserId, CancellationToken cancellationToken = default);

    Task<FamilyShare> AddAsync(FamilyShare familyShare, CancellationToken cancellationToken = default);

    Task<FamilyShare> UpdateAsync(FamilyShare familyShare, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid shareId, CancellationToken cancellationToken = default);
}

