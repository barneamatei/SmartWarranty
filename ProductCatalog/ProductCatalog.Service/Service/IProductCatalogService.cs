using ProductCatalog.Domain.DTOs;

namespace ProductCatalog.Service;

public interface IProductCatalogService
{
    Task<ProductDto> CreateProductAsync(string name, string brand, string model, Guid categoryId);

    Task<ProductDto> UpdateProductAsync(Guid productId, string name, string brand, string model);

    Task<ProductDto> ChangeCategoryAsync(Guid productId, Guid categoryId);

    Task<ProductDto?> GetProductByIdAsync(Guid productId);

    Task<IEnumerable<ProductDto>> GetAllProductsAsync();

    Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(Guid categoryId);

    Task<bool> DeleteProductAsync(Guid productId);
}
