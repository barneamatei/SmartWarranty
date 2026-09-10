using WarrantyManagement.Infrastructure.Persistence;

namespace WarrantyManagement.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(WarrantyManagementDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
    }
}
