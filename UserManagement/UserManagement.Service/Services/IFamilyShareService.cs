using UserManagement.Domain.DTOs;

namespace UserManagement.Service.Services;

public interface IFamilyShareService
{
    Task<FamilyShareResponseDto> CreateAsync(CreateFamilyShareDto dto, CancellationToken cancellationToken = default);

    Task<FamilyShareResponseDto?> GetByIdAsync(Guid shareId, CancellationToken cancellationToken = default);

    Task<IEnumerable<FamilyShareResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<FamilyShareResponseDto>> GetByOwnerIdAsync(Guid ownerUserId, CancellationToken cancellationToken = default);

    Task<IEnumerable<FamilyShareResponseDto>> GetByMemberIdAsync(Guid memberUserId, CancellationToken cancellationToken = default);

    Task<FamilyShareResponseDto> UpdateAsync(Guid shareId, UpdateFamilyShareDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid shareId, CancellationToken cancellationToken = default);
}

