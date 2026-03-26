using Microsoft.EntityFrameworkCore;
using WarrantyManagement.Domain.Entities;
using WarrantyManagement.Infrastructure.Persistence;

namespace WarrantyManagement.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(WarrantyManagementDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Warranties.AnyAsync())
        {
            return;
        }

        var warranties = new[]
        {
            new Warranty(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1"),
                new DateTime(2025, 11, 15),
                24),
            new Warranty(
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"),
                new DateTime(2023, 1, 10),
                12),
            new Warranty(
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
                Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3"),
                new DateTime(2025, 9, 1),
                18),
            new Warranty(
                Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"),
                Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb4"),
                new DateTime(2025, 6, 20),
                36),
            new Warranty(
                Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5"),
                Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb5"),
                new DateTime(2025, 2, 5),
                24)
        };

        warranties[1].RecalculateStatus();
        warranties[3].MarkInactive();

        var openedClaim = new Claim(
            Guid.Parse("66666666-6666-6666-6666-666666666661"),
            warranties[2].WarrantyId,
            "Display flickers intermittently after startup.");
        openedClaim.UpdateStatus(ClaimStatus.InReview);
        warranties[2].MarkClaimed();

        var closedClaim = new Claim(
            Guid.Parse("66666666-6666-6666-6666-666666666662"),
            warranties[4].WarrantyId,
            "Battery no longer charges above 40 percent.");
        closedClaim.UpdateStatus(ClaimStatus.Approved);
        closedClaim.Close();
        warranties[4].RecalculateStatus();

        await context.Warranties.AddRangeAsync(warranties);
        await context.Claims.AddRangeAsync(openedClaim, closedClaim);
        await context.SaveChangesAsync();
    }
}
