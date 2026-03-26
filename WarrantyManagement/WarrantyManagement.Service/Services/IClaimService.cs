using WarrantyManagement.Domain.DTOs;

namespace WarrantyManagement.Service.Services;

public interface IClaimService
{
    Task<ClaimResponseDto> CreateAsync(CreateClaimDto dto, CancellationToken cancellationToken = default);

    Task<ClaimResponseDto?> GetByIdAsync(Guid claimId, CancellationToken cancellationToken = default);

    Task<IEnumerable<ClaimResponseDto>> GetByWarrantyIdAsync(Guid warrantyId, CancellationToken cancellationToken = default);

    Task<ClaimResponseDto> UpdateStatusAsync(Guid claimId, UpdateClaimStatusDto dto, CancellationToken cancellationToken = default);

    Task<ClaimResponseDto> CloseAsync(Guid claimId, CancellationToken cancellationToken = default);
}
