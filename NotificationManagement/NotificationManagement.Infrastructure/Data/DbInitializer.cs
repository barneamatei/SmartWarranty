using NotificationManagement.Infrastructure.Persistence;

namespace NotificationManagement.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(NotificationManagementDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
    }
}
