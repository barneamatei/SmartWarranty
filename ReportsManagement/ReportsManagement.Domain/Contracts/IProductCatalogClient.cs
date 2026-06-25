using ReportsManagement.Domain.DTOs;

namespace ReportsManagement.Domain.Contracts;

public interface IProductCatalogClient
{
    Task<IReadOnlyList<ProductReportDto>> GetProductsAsync(CancellationToken cancellationToken = default);
}
