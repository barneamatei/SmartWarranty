using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Persistence;

public static class DbInitializer
{
    private static readonly Guid AdminUserId = Guid.Parse("34785edf-7ff8-4314-91c6-fa079f5e14a4");
    private static readonly Guid AnaUserId = Guid.Parse("3a69d535-56b5-47f1-b052-a4ff3ba264bf");
    private static readonly Guid MihaiUserId = Guid.Parse("4c3734dd-925c-4548-a232-63af2f0d4220");
    private static readonly Guid ElenaUserId = Guid.Parse("0c00a9da-4c90-434e-b375-b52db5079db0");
    private static readonly Guid VladUserId = Guid.Parse("b4fbc904-3670-4785-a8c1-ef4c654297a3");
    private static readonly Guid TestAdminUserId = Guid.Parse("2c92dea4-781d-4258-ac34-b73355a6c94a");

    public static async Task InitializeAsync(UserManagementDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        await EnsureUserAsync(context, AdminUserId, "admin@smartwarranty.local", "System Admin", "+40700000001", PlanType.Premium, Guid.Parse("82000000-0000-0000-0000-000000000001"));
        await EnsureUserAsync(context, AnaUserId, "ana.popescu@demo.local", "Ana Popescu", "+40721111111", PlanType.Premium, Guid.Parse("82000000-0000-0000-0000-000000000002"));
        await EnsureUserAsync(context, MihaiUserId, "mihai.ionescu@demo.local", "Mihai Ionescu", "+40722222222", PlanType.Premium, Guid.Parse("82000000-0000-0000-0000-000000000003"));
        await EnsureUserAsync(context, ElenaUserId, "elena.marin@demo.local", "Elena Marin", "+40723333333", PlanType.Premium, Guid.Parse("82000000-0000-0000-0000-000000000004"));
        await EnsureUserAsync(context, VladUserId, "vlad.dumitru@demo.local", "Vlad Dumitru", "+40724444444", PlanType.Free, Guid.Parse("82000000-0000-0000-0000-000000000005"));
        await EnsureUserAsync(context, TestAdminUserId, "admin@email.com", "Test Admin", "+40700000002", PlanType.Premium, Guid.Parse("82000000-0000-0000-0000-000000000006"));
        await EnsureUserAsync(context, Guid.Parse("a1000000-0000-0000-0000-000000000001"), "andrei.radu@demo.local", "Andrei Radu", "+40725555551", PlanType.Premium, Guid.Parse("82000000-0000-0000-0000-000000000007"));
        await EnsureUserAsync(context, Guid.Parse("a1000000-0000-0000-0000-000000000002"), "ioana.stan@demo.local", "Ioana Stan", "+40725555552", PlanType.Premium, Guid.Parse("82000000-0000-0000-0000-000000000008"));
        await EnsureUserAsync(context, Guid.Parse("a1000000-0000-0000-0000-000000000003"), "cristian.enache@demo.local", "Cristian Enache", "+40725555553", PlanType.Free, Guid.Parse("82000000-0000-0000-0000-000000000009"));
        await EnsureUserAsync(context, Guid.Parse("a1000000-0000-0000-0000-000000000004"), "maria.georgescu@demo.local", "Maria Georgescu", "+40725555554", PlanType.Premium, Guid.Parse("82000000-0000-0000-0000-000000000010"));
        await EnsureUserAsync(context, Guid.Parse("a1000000-0000-0000-0000-000000000005"), "alexandru.neagu@demo.local", "Alexandru Neagu", "+40725555555", PlanType.Free, Guid.Parse("82000000-0000-0000-0000-000000000011"));
        await EnsureUserAsync(context, Guid.Parse("a1000000-0000-0000-0000-000000000006"), "raluca.pavel@demo.local", "Raluca Pavel", "+40725555556", PlanType.Premium, Guid.Parse("82000000-0000-0000-0000-000000000012"));
        await EnsureUserAsync(context, Guid.Parse("a1000000-0000-0000-0000-000000000007"), "sorin.tudor@demo.local", "Sorin Tudor", "+40725555557", PlanType.Free, Guid.Parse("82000000-0000-0000-0000-000000000013"));
        await EnsureUserAsync(context, Guid.Parse("a1000000-0000-0000-0000-000000000008"), "bianca.ilie@demo.local", "Bianca Ilie", "+40725555558", PlanType.Premium, Guid.Parse("82000000-0000-0000-0000-000000000014"));
        await EnsureUserAsync(context, Guid.Parse("a1000000-0000-0000-0000-000000000009"), "daniel.matei@demo.local", "Daniel Matei", "+40725555559", PlanType.Premium, Guid.Parse("82000000-0000-0000-0000-000000000015"));
        await EnsureUserAsync(context, Guid.Parse("a1000000-0000-0000-0000-000000000010"), "oana.dobre@demo.local", "Oana Dobre", "+40725555560", PlanType.Free, Guid.Parse("82000000-0000-0000-0000-000000000016"));
        await EnsureUserAsync(context, Guid.Parse("a1000000-0000-0000-0000-000000000011"), "stefan.lazar@demo.local", "Stefan Lazar", "+40725555561", PlanType.Premium, Guid.Parse("82000000-0000-0000-0000-000000000017"));
        await EnsureUserAsync(context, Guid.Parse("a1000000-0000-0000-0000-000000000012"), "larisa.voicu@demo.local", "Larisa Voicu", "+40725555562", PlanType.Free, Guid.Parse("82000000-0000-0000-0000-000000000018"));
        await EnsureUserAsync(context, Guid.Parse("a1000000-0000-0000-0000-000000000013"), "bogdan.petrescu@demo.local", "Bogdan Petrescu", "+40725555563", PlanType.Premium, Guid.Parse("82000000-0000-0000-0000-000000000019"));
        await EnsureUserAsync(context, Guid.Parse("a1000000-0000-0000-0000-000000000014"), "irina.serban@demo.local", "Irina Serban", "+40725555564", PlanType.Free, Guid.Parse("82000000-0000-0000-0000-000000000020"));

        await EnsureShareAsync(context, Guid.Parse("81000000-0000-0000-0000-000000000001"), AnaUserId, MihaiUserId, Permissions.View);
        await EnsureShareAsync(context, Guid.Parse("81000000-0000-0000-0000-000000000002"), ElenaUserId, AnaUserId, Permissions.View);
        await EnsureShareAsync(context, Guid.Parse("81000000-0000-0000-0000-000000000003"), MihaiUserId, VladUserId, Permissions.View);

        await context.SaveChangesAsync();
    }

    private static async Task EnsureUserAsync(
        UserManagementDbContext context,
        Guid userId,
        string email,
        string name,
        string phone,
        PlanType planType,
        Guid subscriptionId)
    {
        var user = await context.Users
            .Include(item => item.UserProfile)
            .Include(item => item.Subscription)
            .FirstOrDefaultAsync(item => item.UserId == userId);

        if (user == null)
        {
            user = new User(userId, email);
            await context.Users.AddAsync(user);
        }
        else
        {
            user.UpdateEmail(email);
            user.Activate();
        }

        if (user.UserProfile == null)
        {
            await context.UserProfiles.AddAsync(new UserProfile(userId, name, phone, "ro", "{\"theme\":\"light\",\"notifications\":true}"));
        }
        else
        {
            user.UserProfile.Update(name, phone, "ro", "{\"theme\":\"light\",\"notifications\":true}");
        }

        var subscription = user.Subscription
            ?? await context.Subscriptions.FirstOrDefaultAsync(item => item.UserId == userId);
        var startDate = new DateTime(2026, 1, 1);
        var endDate = planType == PlanType.Premium ? new DateTime(2027, 1, 1) : new DateTime(2026, 12, 31);

        if (subscription == null)
        {
            await context.Subscriptions.AddAsync(new Subscription(subscriptionId, userId, planType, startDate, endDate));
        }
        else if (planType == PlanType.Premium)
        {
            subscription.UpgradeToPremium(endDate);
        }
        else
        {
            subscription.DowngradeToFree(endDate);
        }
    }

    private static async Task EnsureShareAsync(
        UserManagementDbContext context,
        Guid shareId,
        Guid ownerUserId,
        Guid memberUserId,
        Permissions permissions)
    {
        var share = await context.FamilyShares.FirstOrDefaultAsync(item =>
            item.OwnerUserId == ownerUserId && item.MemberUserId == memberUserId);

        if (share == null)
        {
            await context.FamilyShares.AddAsync(new FamilyShare(shareId, ownerUserId, memberUserId, permissions));
        }
        else
        {
            share.UpdatePermissions(permissions);
        }
    }

}
