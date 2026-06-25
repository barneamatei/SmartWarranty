using WarrantyManagement.Domain.Contracts;
using WarrantyManagement.Domain.DTOs;
using WarrantyManagement.Domain.Entities;
using WarrantyManagement.Service.Exceptions;

namespace WarrantyManagement.Service.Services;

public class WarrantyService
{
    private readonly IWarrantyDao _warrantyDao;
    private readonly IDocumentAnalysisClient _documentAnalysisClient;
    private readonly IUserManagementClient _userManagementClient;
    private readonly IProductCatalogClient _productCatalogClient;
    private readonly INotificationManagementClient _notificationManagementClient;

    public WarrantyService(
        IWarrantyDao warrantyDao,
        IDocumentAnalysisClient documentAnalysisClient,
        IUserManagementClient userManagementClient,
        IProductCatalogClient productCatalogClient,
        INotificationManagementClient notificationManagementClient)
    {
        _warrantyDao = warrantyDao ?? throw new ArgumentNullException(nameof(warrantyDao));
        _documentAnalysisClient = documentAnalysisClient ?? throw new ArgumentNullException(nameof(documentAnalysisClient));
        _userManagementClient = userManagementClient ?? throw new ArgumentNullException(nameof(userManagementClient));
        _productCatalogClient = productCatalogClient ?? throw new ArgumentNullException(nameof(productCatalogClient));
        _notificationManagementClient = notificationManagementClient ?? throw new ArgumentNullException(nameof(notificationManagementClient));
    }

    public async Task<WarrantyResponseDto> CreateAsync(CreateWarrantyDto dto, CancellationToken cancellationToken = default)
    {
        await ValidateWarrantyInputAsync(dto.UserId, dto.ProductId, dto.PurchaseDate, dto.DurationMonths, cancellationToken);

        var warranty = new Warranty(Guid.NewGuid(), dto.UserId, dto.ProductId, dto.PurchaseDate, dto.DurationMonths);
        var savedWarranty = await _warrantyDao.AddAsync(warranty, cancellationToken);
        await TryCreateNotificationAsync(
            dto.UserId,
            "Warranty created",
            $"A new warranty was created and expires on {savedWarranty.ExpiryDate:yyyy-MM-dd}.",
            "General",
            $"{{\"warrantyId\":\"{savedWarranty.WarrantyId}\"}}",
            cancellationToken);
        return MapToResponse(savedWarranty);
    }

    public async Task<WarrantyCreationFromDocumentResponseDto> CreateFromDocumentAsync(CreateWarrantyFromDocumentDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.DocumentId == Guid.Empty)
            throw new DomainException("DocumentId is invalid.");

        var createdWarranty = await CreateAsync(new CreateWarrantyDto
        {
            UserId = dto.UserId,
            ProductId = dto.ProductId,
            PurchaseDate = dto.PurchaseDate,
            DurationMonths = dto.DurationMonths
        }, cancellationToken);

        return new WarrantyCreationFromDocumentResponseDto
        {
            DocumentId = dto.DocumentId,
            MerchantName = dto.MerchantName,
            DocumentNumber = dto.DocumentNumber,
            ProductDescription = dto.ProductDescription,
            TotalAmount = dto.TotalAmount,
            Currency = dto.Currency,
            Warranty = createdWarranty
        };
    }

    public async Task<WarrantyCreationFromDocumentResponseDto> CreateFromAnalyzedDocumentAsync(Guid documentId, CreateWarrantyFromAnalyzedDocumentDto dto, CancellationToken cancellationToken = default)
    {
        if (documentId == Guid.Empty)
            throw new DomainException("DocumentId is invalid.");

        var draft = await _documentAnalysisClient.CreateWarrantyDraftAsync(documentId, dto, cancellationToken);

        return await CreateFromDocumentAsync(new CreateWarrantyFromDocumentDto
        {
            DocumentId = draft.DocumentId,
            UserId = draft.UserId,
            ProductId = draft.ProductId,
            PurchaseDate = draft.PurchaseDate,
            DurationMonths = draft.DurationMonths,
            ProductDescription = draft.ProductDescription,
            MerchantName = draft.MerchantName,
            DocumentNumber = draft.DocumentNumber,
            TotalAmount = draft.TotalAmount,
            Currency = draft.Currency
        }, cancellationToken);
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

        await ValidateWarrantyInputAsync(dto.UserId, dto.ProductId, dto.PurchaseDate, dto.DurationMonths, cancellationToken);
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

    private async Task ValidateWarrantyInputAsync(Guid userId, Guid productId, DateTime purchaseDate, int durationMonths, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId is invalid.");
        if (productId == Guid.Empty)
            throw new DomainException("ProductId is invalid.");
        if (purchaseDate == default)
            throw new DomainException("PurchaseDate is invalid.");
        if (durationMonths <= 0)
            throw new DomainException("DurationMonths must be greater than 0.");

        if (!await _userManagementClient.UserExistsAsync(userId, cancellationToken))
            throw new DomainException($"User with ID {userId} was not found in UserManagement.");

        if (!await _productCatalogClient.ProductExistsAsync(productId, cancellationToken))
            throw new DomainException($"Product with ID {productId} was not found in ProductCatalog.");
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

    private async Task TryCreateNotificationAsync(
        Guid userId,
        string title,
        string message,
        string type,
        string metadata,
        CancellationToken cancellationToken)
    {
        try
        {
            await _notificationManagementClient.CreateNotificationAsync(userId, title, message, type, "InApp", metadata, cancellationToken);
        }
        catch (Exception ex)
        {
            _ = ex;
        }
    }
}
