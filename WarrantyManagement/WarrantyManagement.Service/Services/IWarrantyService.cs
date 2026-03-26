using WarrantyManagement.Domain.DTOs;

namespace WarrantyManagement.Service.Services;

public interface IWarrantyService
{
    Task<WarrantyResponseDto> CreateAsync(CreateWarrantyDto dto, CancellationToken cancellationToken = default);

    Task<WarrantyResponseDto?> GetByIdAsync(Guid warrantyId, CancellationToken cancellationToken = default);

    Task<IEnumerable<WarrantyResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<WarrantyResponseDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<WarrantyResponseDto>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<WarrantyResponseDto> UpdateAsync(Guid warrantyId, UpdateWarrantyDto dto, CancellationToken cancellationToken = default);

    Task<WarrantyResponseDto> RefreshStatusAsync(Guid warrantyId, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid warrantyId, CancellationToken cancellationToken = default);
}
