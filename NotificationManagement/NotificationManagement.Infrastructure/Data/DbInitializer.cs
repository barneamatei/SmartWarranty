using NotificationManagement.Domain.Entities;
using NotificationManagement.Infrastructure.Persistence;

namespace NotificationManagement.Infrastructure.Data;

public static class DbInitializer
{
    private static readonly Guid AnaUserId = Guid.Parse("3a69d535-56b5-47f1-b052-a4ff3ba264bf");
    private static readonly Guid MihaiUserId = Guid.Parse("4c3734dd-925c-4548-a232-63af2f0d4220");
    private static readonly Guid ElenaUserId = Guid.Parse("0c00a9da-4c90-434e-b375-b52db5079db0");
    private static readonly Guid VladUserId = Guid.Parse("b4fbc904-3670-4785-a8c1-ef4c654297a3");

    public static async Task InitializeAsync(NotificationManagementDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (!context.Notifications.Any())
        {
            var user1 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var user2 = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

            var notifications = new List<Notification>
            {
                new(Guid.Parse("10000000-0000-0000-0000-000000000001"), user1, "Warranty expires soon", "Your warranty for Laptop Dell Inspiron 15 expires in 10 days.", NotificationType.WarrantyExpiring, NotificationChannel.InApp, "{\"warrantyId\":\"11111111-1111-1111-1111-111111111111\"}"),
                new(Guid.Parse("10000000-0000-0000-0000-000000000002"), user1, "Claim updated", "Your warranty claim has moved to InReview.", NotificationType.ClaimUpdated, NotificationChannel.Email, "{\"claimId\":\"66666666-6666-6666-6666-666666666661\"}"),
                new(Guid.Parse("10000000-0000-0000-0000-000000000003"), user2, "Document processed", "Your invoice was analyzed successfully.", NotificationType.DocumentProcessed, NotificationChannel.InApp, "{\"documentId\":\"b6be1d83-ccbb-450e-9922-4222fe5effdd\"}"),
                new(Guid.Parse("10000000-0000-0000-0000-000000000004"), user2, "System maintenance", "Scheduled maintenance will take place tonight at 23:00.", NotificationType.System, NotificationChannel.Email),
                new(Guid.Parse("10000000-0000-0000-0000-000000000005"), user1, "Welcome", "Your SmartWarranty account is ready.", NotificationType.General, NotificationChannel.InApp)
            };

            notifications[0].MarkSent();
            notifications[1].MarkSent();
            notifications[2].MarkSent();
            notifications[2].MarkRead();
            notifications[3].MarkFailed("SMTP provider unavailable.");

            context.Notifications.AddRange(notifications);
            await context.SaveChangesAsync();
        }

        await EnsureDemoNotificationsAsync(context);
    }

    private static async Task EnsureDemoNotificationsAsync(NotificationManagementDbContext context)
    {
        var notifications = new List<Notification>
        {
            new(Guid.Parse("75000000-0000-0000-0000-000000000001"), AnaUserId, "iPhone warranty expires soon", "The warranty for iPhone 15 Pro expires on 2026-07-03.", NotificationType.WarrantyExpiring, NotificationChannel.InApp, "{\"warrantyId\":\"73000000-0000-0000-0000-000000000002\"}"),
            new(Guid.Parse("75000000-0000-0000-0000-000000000002"), AnaUserId, "Family share active", "Mihai can now view the products and warranties shared from your account.", NotificationType.General, NotificationChannel.InApp),
            new(Guid.Parse("75000000-0000-0000-0000-000000000003"), MihaiUserId, "Monitor warranty expires soon", "Dell UltraSharp warranty expires on 2026-07-18.", NotificationType.WarrantyExpiring, NotificationChannel.Email, "{\"warrantyId\":\"73000000-0000-0000-0000-000000000004\"}"),
            new(Guid.Parse("75000000-0000-0000-0000-000000000004"), ElenaUserId, "Claim updated", "Your Sony Alpha 7 IV claim is now InReview.", NotificationType.ClaimUpdated, NotificationChannel.InApp, "{\"claimId\":\"74000000-0000-0000-0000-000000000001\"}"),
            new(Guid.Parse("75000000-0000-0000-0000-000000000005"), VladUserId, "Shared warranties available", "Mihai shared his warranty portfolio with you.", NotificationType.General, NotificationChannel.InApp)
        };

        notifications[0].MarkSent();
        notifications[1].MarkRead();
        notifications[2].MarkSent();
        notifications[3].MarkSent();

        foreach (var notification in notifications)
        {
            if (!context.Notifications.Any(item => item.NotificationId == notification.NotificationId))
                context.Notifications.Add(notification);
        }

        await context.SaveChangesAsync();
    }
}
