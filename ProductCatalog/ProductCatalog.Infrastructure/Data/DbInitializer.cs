using Microsoft.EntityFrameworkCore;

namespace ProductCatalog.Infrastructure.Data;

public static class DbInitializer
{
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
    }
}
