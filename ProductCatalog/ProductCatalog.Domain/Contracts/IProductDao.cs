using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Domain.Contracts;

public interface IProductDao
{
    Task<Product?> GetByIdAsync(Guid productId);

    Task<IEnumerable<Product>> GetAllAsync();

    Task<IEnumerable<Product>> GetByUserIdAsync(Guid userId);

    Task<Product> AddAsync(Product product);

    Task<Product> UpdateAsync(Product product);

    Task<bool> DeleteAsync(Guid productId);
}
