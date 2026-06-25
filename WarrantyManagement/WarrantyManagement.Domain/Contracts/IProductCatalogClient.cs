namespace WarrantyManagement.Domain.Contracts;

public interface IProductCatalogClient
{
    Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken = default);
}
