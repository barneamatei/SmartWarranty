using ProductCatalog.Domain.Contracts;
using ProductCatalog.Domain.DTOs;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Service;

public class ProductCatalogService : IProductCatalogService
{
    private readonly IProductDao _productRepository;
    private readonly ICategoryDao _categoryRepository;

    public ProductCatalogService(
        IProductDao productRepository,
        ICategoryDao categoryRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
    }

    public async Task<ProductDto> CreateProductAsync(string name, string brand, string model, Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be null or empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(brand))
            throw new ArgumentException("Product brand cannot be null or empty.", nameof(brand));
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Product model cannot be null or empty.", nameof(model));
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category ID cannot be empty.", nameof(categoryId));

        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new InvalidOperationException($"Category with ID {categoryId} does not exist.");
        }

        var productId = Guid.NewGuid();
        var product = new Product(productId, name, brand, model, categoryId);

        var savedProduct = await _productRepository.AddAsync(product);
        return MapToDto(savedProduct);
    }

    public async Task<ProductDto> UpdateProductAsync(Guid productId, string name, string brand, string model)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be null or empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(brand))
            throw new ArgumentException("Product brand cannot be null or empty.", nameof(brand));
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Product model cannot be null or empty.", nameof(model));

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {productId} does not exist.");
        }

        product.UpdateDetails(name, brand, model);

        var updatedProduct = await _productRepository.UpdateAsync(product);
        return MapToDto(updatedProduct);
    }

    public async Task<ProductDto> ChangeCategoryAsync(Guid productId, Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category ID cannot be empty.", nameof(categoryId));

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {productId} does not exist.");
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new InvalidOperationException($"Category with ID {categoryId} does not exist.");
        }

        product.ChangeCategory(categoryId);

        var updatedProduct = await _productRepository.UpdateAsync(product);
        return MapToDto(updatedProduct);
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            return null;
        }
        return MapToDto(product);
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(Guid categoryId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new InvalidOperationException($"Category with ID {categoryId} does not exist.");
        }

        var allProducts = await _productRepository.GetAllAsync();
        return allProducts.Where(p => p.CategoryId == categoryId).Select(MapToDto);
    }

    public async Task<bool> DeleteProductAsync(Guid productId)
    {
        return await _productRepository.DeleteAsync(productId);
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            ProductId = product.ProductId,
            Name = product.Name,
            Brand = product.Brand,
            Model = product.Model,
            CategoryId = product.CategoryId,
            Status = product.Status.ToString()
        };
    }
}



