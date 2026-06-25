using Microsoft.EntityFrameworkCore;
using WarrantyManagement.Domain.Entities;
using WarrantyManagement.Infrastructure.Persistence;

namespace WarrantyManagement.Infrastructure.Data;

public static class DbInitializer
{
    private static readonly Guid AnaUserId = Guid.Parse("3a69d535-56b5-47f1-b052-a4ff3ba264bf");
    private static readonly Guid MihaiUserId = Guid.Parse("4c3734dd-925c-4548-a232-63af2f0d4220");
    private static readonly Guid ElenaUserId = Guid.Parse("0c00a9da-4c90-434e-b375-b52db5079db0");
    private static readonly Guid VladUserId = Guid.Parse("b4fbc904-3670-4785-a8c1-ef4c654297a3");

    public static async Task InitializeAsync(WarrantyManagementDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (!await context.Warranties.AnyAsync())
        {
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

        await EnsureDemoWarrantiesAsync(context);
    }

    private static async Task EnsureDemoWarrantiesAsync(WarrantyManagementDbContext context)
    {
        var warranties = new[]
        {
            new Warranty(Guid.Parse("73000000-0000-0000-0000-000000000001"), AnaUserId, Guid.Parse("72000000-0000-0000-0000-000000000001"), new DateTime(2025, 9, 10), 24),
            new Warranty(Guid.Parse("73000000-0000-0000-0000-000000000002"), AnaUserId, Guid.Parse("72000000-0000-0000-0000-000000000002"), new DateTime(2024, 7, 3), 24),
            new Warranty(Guid.Parse("73000000-0000-0000-0000-000000000003"), AnaUserId, Guid.Parse("72000000-0000-0000-0000-000000000003"), new DateTime(2023, 3, 15), 24),
            new Warranty(Guid.Parse("73000000-0000-0000-0000-000000000004"), MihaiUserId, Guid.Parse("72000000-0000-0000-0000-000000000004"), new DateTime(2024, 7, 18), 24),
            new Warranty(Guid.Parse("73000000-0000-0000-0000-000000000005"), MihaiUserId, Guid.Parse("72000000-0000-0000-0000-000000000005"), new DateTime(2025, 2, 1), 36),
            new Warranty(Guid.Parse("73000000-0000-0000-0000-000000000006"), MihaiUserId, Guid.Parse("72000000-0000-0000-0000-000000000006"), new DateTime(2023, 12, 20), 12),
            new Warranty(Guid.Parse("73000000-0000-0000-0000-000000000007"), ElenaUserId, Guid.Parse("72000000-0000-0000-0000-000000000007"), new DateTime(2025, 8, 12), 24),
            new Warranty(Guid.Parse("73000000-0000-0000-0000-000000000008"), ElenaUserId, Guid.Parse("72000000-0000-0000-0000-000000000008"), new DateTime(2024, 6, 29), 24),
            new Warranty(Guid.Parse("73000000-0000-0000-0000-000000000009"), VladUserId, Guid.Parse("72000000-0000-0000-0000-000000000009"), new DateTime(2025, 6, 1), 12)
        };

        warranties[6].MarkClaimed();

        foreach (var warranty in warranties)
        {
            if (!await context.Warranties.AnyAsync(item => item.WarrantyId == warranty.WarrantyId))
                await context.Warranties.AddAsync(warranty);
        }

        if (!await context.Claims.AnyAsync(item => item.ClaimId == Guid.Parse("74000000-0000-0000-0000-000000000001")))
        {
            var claim = new Claim(
                Guid.Parse("74000000-0000-0000-0000-000000000001"),
                Guid.Parse("73000000-0000-0000-0000-000000000007"),
                "Camera reports intermittent autofocus errors during travel shoots.");
            claim.UpdateStatus(ClaimStatus.InReview);
            await context.Claims.AddAsync(claim);
        }

        await context.SaveChangesAsync();
    }
}
