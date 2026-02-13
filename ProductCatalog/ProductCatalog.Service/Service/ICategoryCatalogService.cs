using ProductCatalog.Domain.DTOs;

namespace ProductCatalog.Service;

public interface ICategoryCatalogService
{
    Task<CategoryDto> CreateCategoryAsync(string name, string description);

    Task<CategoryDto> UpdateCategoryAsync(Guid categoryId, string name, string description);

    Task<CategoryDto?> GetCategoryByIdAsync(Guid categoryId);

    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();

    Task<bool> DeleteCategoryAsync(Guid categoryId);
}
