using WarrantyManagement.Domain.Contracts;
using WarrantyManagement.Domain.DTOs;
using WarrantyManagement.Domain.Entities;
using WarrantyManagement.Service.Exceptions;

namespace WarrantyManagement.Service.Services;

public class WarrantyService : IWarrantyService
{
    private readonly IWarrantyDao _warrantyDao;

    public WarrantyService(IWarrantyDao warrantyDao)
    {
        _warrantyDao = warrantyDao ?? throw new ArgumentNullException(nameof(warrantyDao));
    }

    public async Task<WarrantyResponseDto> CreateAsync(CreateWarrantyDto dto, CancellationToken cancellationToken = default)
    {
        ValidateWarrantyInput(dto.UserId, dto.ProductId, dto.PurchaseDate, dto.DurationMonths);

        var warranty = new Warranty(Guid.NewGuid(), dto.UserId, dto.ProductId, dto.PurchaseDate, dto.DurationMonths);
        var savedWarranty = await _warrantyDao.AddAsync(warranty, cancellationToken);
        return MapToResponse(savedWarranty);
    }

    public async Task<WarrantyResponseDto?> GetByIdAsync(Guid warrantyId, CancellationToken cancellationToken = default)
    {
        var warranty = await _warrantyDao.GetByIdAsync(warrantyId, cancellationToken);
        return warranty == null ? null : MapToResponse(warranty);
    }

    public async Task<IEnumerable<WarrantyResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var warranties = await _warrantyDao.GetAllAsync(cancellationToken);
        return warranties.Select(MapToResponse);
    }

    public async Task<IEnumerable<WarrantyResponseDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId is invalid.");

        var warranties = await _warrantyDao.GetByUserIdAsync(userId, cancellationToken);
        return warranties.Select(MapToResponse);
    }

    public async Task<IEnumerable<WarrantyResponseDto>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        if (productId == Guid.Empty)
            throw new DomainException("ProductId is invalid.");

        var warranties = await _warrantyDao.GetByProductIdAsync(productId, cancellationToken);
        return warranties.Select(MapToResponse);
    }

    public async Task<WarrantyResponseDto> UpdateAsync(Guid warrantyId, UpdateWarrantyDto dto, CancellationToken cancellationToken = default)
    {
        var warranty = await _warrantyDao.GetByIdAsync(warrantyId, cancellationToken);
        if (warranty == null)
            throw new DomainException($"Warranty with ID {warrantyId} not found.");

        ValidateWarrantyInput(dto.UserId, dto.ProductId, dto.PurchaseDate, dto.DurationMonths);
        warranty.UpdateDetails(dto.UserId, dto.ProductId, dto.PurchaseDate, dto.DurationMonths);

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            if (!Enum.TryParse<WarrantyStatus>(dto.Status, true, out var parsedStatus))
                throw new DomainException("Warranty status is invalid.");

            ApplyStatusOverride(warranty, parsedStatus);
        }
        else
        {
            warranty.RecalculateStatus();
        }

        var updatedWarranty = await _warrantyDao.UpdateAsync(warranty, cancellationToken);
        return MapToResponse(updatedWarranty);
    }

    public async Task<WarrantyResponseDto> RefreshStatusAsync(Guid warrantyId, CancellationToken cancellationToken = default)
    {
        var warranty = await _warrantyDao.GetByIdAsync(warrantyId, cancellationToken);
        if (warranty == null)
            throw new DomainException($"Warranty with ID {warrantyId} not found.");

        warranty.RecalculateStatus();
        var updatedWarranty = await _warrantyDao.UpdateAsync(warranty, cancellationToken);
        return MapToResponse(updatedWarranty);
    }

    public Task<bool> DeleteAsync(Guid warrantyId, CancellationToken cancellationToken = default)
    {
        return _warrantyDao.DeleteAsync(warrantyId, cancellationToken);
    }

    private static void ValidateWarrantyInput(Guid userId, Guid productId, DateTime purchaseDate, int durationMonths)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId is invalid.");
        if (productId == Guid.Empty)
            throw new DomainException("ProductId is invalid.");
        if (purchaseDate == default)
            throw new DomainException("PurchaseDate is invalid.");
        if (durationMonths <= 0)
            throw new DomainException("DurationMonths must be greater than 0.");
    }

    private static void ApplyStatusOverride(Warranty warranty, WarrantyStatus status)
    {
        switch (status)
        {
            case WarrantyStatus.Expired:
                if (warranty.ExpiryDate.Date >= DateTime.UtcNow.Date)
                    throw new DomainException("Warranty cannot be marked as Expired before its expiry date.");
                warranty.MarkExpired();
                break;
            case WarrantyStatus.Inactive:
                warranty.MarkInactive();
                break;
            case WarrantyStatus.Claimed:
                warranty.MarkClaimed();
                break;
            case WarrantyStatus.Active:
                warranty.RecalculateStatus();
                break;
            default:
                throw new DomainException("Warranty status is invalid.");
        }
    }

    private static WarrantyResponseDto MapToResponse(Warranty warranty)
    {
        return new WarrantyResponseDto
        {
            WarrantyId = warranty.WarrantyId,
            UserId = warranty.UserId,
            ProductId = warranty.ProductId,
            PurchaseDate = warranty.PurchaseDate,
            DurationMonths = warranty.DurationMonths,
            ExpiryDate = warranty.ExpiryDate,
            Status = warranty.Status.ToString()
        };
    }
}
