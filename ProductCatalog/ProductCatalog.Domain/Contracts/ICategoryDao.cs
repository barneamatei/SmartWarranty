using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Domain.Contracts;

public interface ICategoryDao
{
    Task<Category?> GetByIdAsync(Guid categoryId);

    Task<IEnumerable<Category>> GetAllAsync();

    Task<Category> AddAsync(Category category);

    Task<Category> UpdateAsync(Category category);

    Task<bool> DeleteAsync(Guid categoryId);
}
