using WarrantyManagement.Domain.Contracts;
using WarrantyManagement.Domain.DTOs;
using WarrantyManagement.Domain.Entities;
using WarrantyManagement.Service.Exceptions;

namespace WarrantyManagement.Service.Services;

public class ClaimService : IClaimService
{
    private readonly IClaimDao _claimDao;
    private readonly IWarrantyDao _warrantyDao;

    public ClaimService(IClaimDao claimDao, IWarrantyDao warrantyDao)
    {
        _claimDao = claimDao ?? throw new ArgumentNullException(nameof(claimDao));
        _warrantyDao = warrantyDao ?? throw new ArgumentNullException(nameof(warrantyDao));
    }

    public async Task<ClaimResponseDto> CreateAsync(CreateClaimDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.WarrantyId == Guid.Empty)
            throw new DomainException("WarrantyId is invalid.");
        if (string.IsNullOrWhiteSpace(dto.Description))
            throw new DomainException("Description is required.");

        var warranty = await _warrantyDao.GetByIdAsync(dto.WarrantyId, cancellationToken);
        if (warranty == null)
            throw new DomainException($"Warranty with ID {dto.WarrantyId} not found.");
        if (warranty.Status is WarrantyStatus.Expired or WarrantyStatus.Inactive)
            throw new DomainException("Claim is invalid for the current warranty status.");

        var claim = new Claim(Guid.NewGuid(), dto.WarrantyId, dto.Description.Trim());
        var savedClaim = await _claimDao.AddAsync(claim, cancellationToken);

        warranty.MarkClaimed();
        await _warrantyDao.UpdateAsync(warranty, cancellationToken);

        return MapToResponse(savedClaim);
    }

    public async Task<ClaimResponseDto?> GetByIdAsync(Guid claimId, CancellationToken cancellationToken = default)
    {
        var claim = await _claimDao.GetByIdAsync(claimId, cancellationToken);
        return claim == null ? null : MapToResponse(claim);
    }

    public async Task<IEnumerable<ClaimResponseDto>> GetByWarrantyIdAsync(Guid warrantyId, CancellationToken cancellationToken = default)
    {
        if (warrantyId == Guid.Empty)
            throw new DomainException("WarrantyId is invalid.");

        var warranty = await _warrantyDao.GetByIdAsync(warrantyId, cancellationToken);
        if (warranty == null)
            throw new DomainException($"Warranty with ID {warrantyId} not found.");

        var claims = await _claimDao.GetByWarrantyIdAsync(warrantyId, cancellationToken);
        return claims.Select(MapToResponse);
    }

    public async Task<ClaimResponseDto> UpdateStatusAsync(Guid claimId, UpdateClaimStatusDto dto, CancellationToken cancellationToken = default)
    {
        var claim = await _claimDao.GetByIdAsync(claimId, cancellationToken);
        if (claim == null)
            throw new DomainException($"Claim with ID {claimId} not found.");
        if (claim.Status == ClaimStatus.Closed)
            throw new DomainException($"Claim with ID {claimId} is already closed.");
        if (string.IsNullOrWhiteSpace(dto.Status))
            throw new DomainException("Claim status is invalid.");
        if (!Enum.TryParse<ClaimStatus>(dto.Status, true, out var parsedStatus))
            throw new DomainException("Claim status is invalid.");

        if (parsedStatus == ClaimStatus.Closed)
            return await CloseAsync(claimId, cancellationToken);

        claim.UpdateStatus(parsedStatus);
        var updatedClaim = await _claimDao.UpdateAsync(claim, cancellationToken);
        await RefreshWarrantyStatusAsync(updatedClaim.WarrantyId, parsedStatus, cancellationToken);
        return MapToResponse(updatedClaim);
    }

    public async Task<ClaimResponseDto> CloseAsync(Guid claimId, CancellationToken cancellationToken = default)
    {
        var claim = await _claimDao.GetByIdAsync(claimId, cancellationToken);
        if (claim == null)
            throw new DomainException($"Claim with ID {claimId} not found.");
        if (claim.Status == ClaimStatus.Closed)
            throw new DomainException($"Claim with ID {claimId} is already closed.");

        claim.Close();
        var updatedClaim = await _claimDao.UpdateAsync(claim, cancellationToken);
        await RefreshWarrantyStatusAsync(updatedClaim.WarrantyId, ClaimStatus.Closed, cancellationToken);
        return MapToResponse(updatedClaim);
    }

    private async Task RefreshWarrantyStatusAsync(Guid warrantyId, ClaimStatus claimStatus, CancellationToken cancellationToken)
    {
        var warranty = await _warrantyDao.GetByIdAsync(warrantyId, cancellationToken);
        if (warranty == null)
            throw new DomainException($"Warranty with ID {warrantyId} not found.");

        if (claimStatus == ClaimStatus.Closed)
            warranty.RecalculateStatus();
        else
            warranty.MarkClaimed();

        await _warrantyDao.UpdateAsync(warranty, cancellationToken);
    }

    private static ClaimResponseDto MapToResponse(Claim claim)
    {
        return new ClaimResponseDto
        {
            ClaimId = claim.ClaimId,
            WarrantyId = claim.WarrantyId,
            Status = claim.Status.ToString(),
            OpenedAt = claim.OpenedAt,
            ClosedAt = claim.ClosedAt,
            Description = claim.Description
        };
    }
}
