using ProductCatalog.Domain.Contracts;
using ProductCatalog.Domain.DTOs;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Service;

public class CategoryCatalogService : ICategoryCatalogService
{
    private readonly ICategoryDao _categoryRepository;
    private readonly IProductDao _productRepository;

    public CategoryCatalogService(
        ICategoryDao categoryRepository,
        IProductDao productRepository)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<CategoryDto> CreateCategoryAsync(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be null or empty.", nameof(name));

        var categoryId = Guid.NewGuid();
        var category = new Category(categoryId, name, description ?? string.Empty);

        var savedCategory = await _categoryRepository.AddAsync(category);
        return MapToDto(savedCategory);
    }

    public async Task<CategoryDto> UpdateCategoryAsync(Guid categoryId, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be null or empty.", nameof(name));

        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new InvalidOperationException($"Category with ID {categoryId} does not exist.");
        }

        category.Rename(name);
        category.UpdateDescription(description ?? string.Empty);
        var updatedCategory = await _categoryRepository.UpdateAsync(category);
        return MapToDto(updatedCategory);
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid categoryId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
        {
            return null;
        }
        return MapToDto(category);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(MapToDto);
    }

    public async Task<bool> DeleteCategoryAsync(Guid categoryId)
    {
        var products = await _productRepository.GetAllAsync();
        var hasProducts = products.Any(p => p.CategoryId == categoryId);

        if (hasProducts)
        {
            throw new InvalidOperationException($"Cannot delete category with ID {categoryId} because it has associated products.");
        }

        return await _categoryRepository.DeleteAsync(categoryId);
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            Description = category.Description
        };
    }
}



