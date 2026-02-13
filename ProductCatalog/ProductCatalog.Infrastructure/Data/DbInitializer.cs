using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ProductCatalogDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Categories.AnyAsync())
        {
            return;
        }

        var categories = new[]
        {
            new Category(Guid.NewGuid(), "Electronics", "Electronic devices and components"),
            new Category(Guid.NewGuid(), "Appliances", "Home and kitchen appliances"),
            new Category(Guid.NewGuid(), "Furniture", "Home and office furniture"),
            new Category(Guid.NewGuid(), "Clothing", "Apparel and accessories"),
            new Category(Guid.NewGuid(), "Sports", "Sports equipment and gear")
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        var electronicsCategory = categories[0];
        var appliancesCategory = categories[1];
        var furnitureCategory = categories[2];
        var clothingCategory = categories[3];
        var sportsCategory = categories[4];

        var products = new[]
        {
            new Product(Guid.NewGuid(), "Laptop Dell XPS 15", "Dell", "XPS 15", electronicsCategory.CategoryId),
            new Product(Guid.NewGuid(), "iPhone 15 Pro", "Apple", "iPhone 15 Pro", electronicsCategory.CategoryId),
            new Product(Guid.NewGuid(), "Samsung Galaxy S24", "Samsung", "Galaxy S24", electronicsCategory.CategoryId),
            new Product(Guid.NewGuid(), "MacBook Pro 16", "Apple", "MacBook Pro 16", electronicsCategory.CategoryId),
            new Product(Guid.NewGuid(), "Sony WH-1000XM5", "Sony", "WH-1000XM5", electronicsCategory.CategoryId),
            new Product(Guid.NewGuid(), "Refrigerator Samsung", "Samsung", "RF28R7351SG", appliancesCategory.CategoryId),
            new Product(Guid.NewGuid(), "Washing Machine LG", "LG", "WM3900HWA", appliancesCategory.CategoryId),
            new Product(Guid.NewGuid(), "Dishwasher Bosch", "Bosch", "SHPM65Z55N", appliancesCategory.CategoryId),
            new Product(Guid.NewGuid(), "Microwave Oven Panasonic", "Panasonic", "NN-SN966S", appliancesCategory.CategoryId),
            new Product(Guid.NewGuid(), "Office Chair Ergonomic", "Herman Miller", "Aeron", furnitureCategory.CategoryId),
            new Product(Guid.NewGuid(), "Desk Standing", "Uplift", "V2", furnitureCategory.CategoryId),
            new Product(Guid.NewGuid(), "Sofa 3-Seater", "IKEA", "KIVIK", furnitureCategory.CategoryId),
            new Product(Guid.NewGuid(), "Dining Table Oak", "West Elm", "Mid-Century", furnitureCategory.CategoryId),
            new Product(Guid.NewGuid(), "T-Shirt Cotton", "Nike", "Dri-FIT", clothingCategory.CategoryId),
            new Product(Guid.NewGuid(), "Jeans Classic", "Levi's", "501", clothingCategory.CategoryId),
            new Product(Guid.NewGuid(), "Running Shoes", "Adidas", "Ultraboost 22", clothingCategory.CategoryId),
            new Product(Guid.NewGuid(), "Basketball", "Spalding", "NBA Official", sportsCategory.CategoryId),
            new Product(Guid.NewGuid(), "Tennis Racket", "Wilson", "Blade 98", sportsCategory.CategoryId),
            new Product(Guid.NewGuid(), "Yoga Mat", "Lululemon", "The Reversible Mat", sportsCategory.CategoryId)
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}
