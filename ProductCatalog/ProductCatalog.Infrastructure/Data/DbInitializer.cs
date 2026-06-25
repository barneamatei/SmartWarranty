using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Infrastructure.Data;

public static class DbInitializer
{
    private static readonly Guid AnaUserId = Guid.Parse("3a69d535-56b5-47f1-b052-a4ff3ba264bf");
    private static readonly Guid MihaiUserId = Guid.Parse("4c3734dd-925c-4548-a232-63af2f0d4220");
    private static readonly Guid ElenaUserId = Guid.Parse("0c00a9da-4c90-434e-b375-b52db5079db0");
    private static readonly Guid VladUserId = Guid.Parse("b4fbc904-3670-4785-a8c1-ef4c654297a3");

    public static async Task InitializeAsync(ProductCatalogDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
        await context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH('Products', 'UserId') IS NULL
            BEGIN
                ALTER TABLE Products ADD UserId uniqueidentifier NULL;
            END
            """);
        await context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH('Categories', 'UserId') IS NULL
            BEGIN
                ALTER TABLE Categories ADD UserId uniqueidentifier NULL;
            END
            """);

        if (!await context.Categories.AnyAsync())
        {
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

        await EnsureDemoCatalogAsync(context);
    }

    private static async Task EnsureDemoCatalogAsync(ProductCatalogDbContext context)
    {
        var categories = new[]
        {
            new Category(Guid.Parse("71000000-0000-0000-0000-000000000001"), "Electronice Ana", "Laptop, telefon si accesorii premium", AnaUserId),
            new Category(Guid.Parse("71000000-0000-0000-0000-000000000002"), "Casa Ana", "Electrocasnice si produse pentru locuinta", AnaUserId),
            new Category(Guid.Parse("71000000-0000-0000-0000-000000000003"), "Home Office", "Echipamente pentru birou", MihaiUserId),
            new Category(Guid.Parse("71000000-0000-0000-0000-000000000004"), "Electrocasnice Mihai", "Produse utile in casa", MihaiUserId),
            new Category(Guid.Parse("71000000-0000-0000-0000-000000000005"), "Foto Travel", "Aparate foto si echipamente de calatorie", ElenaUserId),
            new Category(Guid.Parse("71000000-0000-0000-0000-000000000006"), "Personal Vlad", "Produse pentru cont free", VladUserId)
        };

        foreach (var category in categories)
        {
            if (!await context.Categories.AnyAsync(item => item.CategoryId == category.CategoryId))
                await context.Categories.AddAsync(category);
        }

        var products = new[]
        {
            new Product(Guid.Parse("72000000-0000-0000-0000-000000000001"), "MacBook Pro 14", "Apple", "M3 Pro", Guid.Parse("71000000-0000-0000-0000-000000000001"), AnaUserId),
            new Product(Guid.Parse("72000000-0000-0000-0000-000000000002"), "iPhone 15 Pro", "Apple", "A3102", Guid.Parse("71000000-0000-0000-0000-000000000001"), AnaUserId),
            new Product(Guid.Parse("72000000-0000-0000-0000-000000000003"), "Espressor Magnifica", "DeLonghi", "Evo ECAM290", Guid.Parse("71000000-0000-0000-0000-000000000002"), AnaUserId),
            new Product(Guid.Parse("72000000-0000-0000-0000-000000000004"), "Monitor UltraSharp", "Dell", "U2723QE", Guid.Parse("71000000-0000-0000-0000-000000000003"), MihaiUserId),
            new Product(Guid.Parse("72000000-0000-0000-0000-000000000005"), "Aspirator V15", "Dyson", "Detect Absolute", Guid.Parse("71000000-0000-0000-0000-000000000004"), MihaiUserId),
            new Product(Guid.Parse("72000000-0000-0000-0000-000000000006"), "PlayStation 5", "Sony", "CFI-1216A", Guid.Parse("71000000-0000-0000-0000-000000000003"), MihaiUserId),
            new Product(Guid.Parse("72000000-0000-0000-0000-000000000007"), "Camera Alpha 7 IV", "Sony", "ILCE-7M4", Guid.Parse("71000000-0000-0000-0000-000000000005"), ElenaUserId),
            new Product(Guid.Parse("72000000-0000-0000-0000-000000000008"), "AirPods Pro", "Apple", "2nd Gen USB-C", Guid.Parse("71000000-0000-0000-0000-000000000005"), ElenaUserId),
            new Product(Guid.Parse("72000000-0000-0000-0000-000000000009"), "Kindle Paperwhite", "Amazon", "11th Gen", Guid.Parse("71000000-0000-0000-0000-000000000006"), VladUserId)
        };

        foreach (var product in products)
        {
            if (!await context.Products.AnyAsync(item => item.ProductId == product.ProductId))
                await context.Products.AddAsync(product);
        }

        await context.SaveChangesAsync();
    }
}
