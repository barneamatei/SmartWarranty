namespace UserManagement.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(UserManagementDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
    }
}
