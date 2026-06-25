using IdentityManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace IdentityManagement.Infrastructure.Data;

public static class DbInitializer
{
    private static readonly string[] Roles = ["Admin", "User", "SupportAgent"];
    private const string DefaultAdminEmail = "admin@smartwarranty.local";
    private const string DefaultAdminPassword = "Admin123!";
    private const string TestAdminEmail = "admin@email.com";
    private const string TestAdminPassword = "password";
    private const string DemoUserPassword = "Demo123!";

    public static async Task InitializeAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        await EnsureAdminUserAsync(
            userManager,
            Guid.Parse("34785edf-7ff8-4314-91c6-fa079f5e14a4"),
            DefaultAdminEmail,
            DefaultAdminPassword,
            "System",
            "Admin");

        await EnsureAdminUserAsync(
            userManager,
            Guid.Parse("2c92dea4-781d-4258-ac34-b73355a6c94a"),
            TestAdminEmail,
            TestAdminPassword,
            "Test",
            "Admin");

        await EnsureUserAsync(userManager, Guid.Parse("3a69d535-56b5-47f1-b052-a4ff3ba264bf"), "ana.popescu@demo.local", DemoUserPassword, "Ana", "Popescu", "User");
        await EnsureUserAsync(userManager, Guid.Parse("4c3734dd-925c-4548-a232-63af2f0d4220"), "mihai.ionescu@demo.local", DemoUserPassword, "Mihai", "Ionescu", "User");
        await EnsureUserAsync(userManager, Guid.Parse("0c00a9da-4c90-434e-b375-b52db5079db0"), "elena.marin@demo.local", DemoUserPassword, "Elena", "Marin", "User");
        await EnsureUserAsync(userManager, Guid.Parse("b4fbc904-3670-4785-a8c1-ef4c654297a3"), "vlad.dumitru@demo.local", DemoUserPassword, "Vlad", "Dumitru", "User");
        await EnsureUserAsync(userManager, Guid.Parse("a1000000-0000-0000-0000-000000000001"), "andrei.radu@demo.local", DemoUserPassword, "Andrei", "Radu", "User");
        await EnsureUserAsync(userManager, Guid.Parse("a1000000-0000-0000-0000-000000000002"), "ioana.stan@demo.local", DemoUserPassword, "Ioana", "Stan", "User");
        await EnsureUserAsync(userManager, Guid.Parse("a1000000-0000-0000-0000-000000000003"), "cristian.enache@demo.local", DemoUserPassword, "Cristian", "Enache", "User");
        await EnsureUserAsync(userManager, Guid.Parse("a1000000-0000-0000-0000-000000000004"), "maria.georgescu@demo.local", DemoUserPassword, "Maria", "Georgescu", "User");
        await EnsureUserAsync(userManager, Guid.Parse("a1000000-0000-0000-0000-000000000005"), "alexandru.neagu@demo.local", DemoUserPassword, "Alexandru", "Neagu", "User");
        await EnsureUserAsync(userManager, Guid.Parse("a1000000-0000-0000-0000-000000000006"), "raluca.pavel@demo.local", DemoUserPassword, "Raluca", "Pavel", "User");
        await EnsureUserAsync(userManager, Guid.Parse("a1000000-0000-0000-0000-000000000007"), "sorin.tudor@demo.local", DemoUserPassword, "Sorin", "Tudor", "User");
        await EnsureUserAsync(userManager, Guid.Parse("a1000000-0000-0000-0000-000000000008"), "bianca.ilie@demo.local", DemoUserPassword, "Bianca", "Ilie", "User");
        await EnsureUserAsync(userManager, Guid.Parse("a1000000-0000-0000-0000-000000000009"), "daniel.matei@demo.local", DemoUserPassword, "Daniel", "Matei", "User");
        await EnsureUserAsync(userManager, Guid.Parse("a1000000-0000-0000-0000-000000000010"), "oana.dobre@demo.local", DemoUserPassword, "Oana", "Dobre", "User");
        await EnsureUserAsync(userManager, Guid.Parse("a1000000-0000-0000-0000-000000000011"), "stefan.lazar@demo.local", DemoUserPassword, "Stefan", "Lazar", "User");
        await EnsureUserAsync(userManager, Guid.Parse("a1000000-0000-0000-0000-000000000012"), "larisa.voicu@demo.local", DemoUserPassword, "Larisa", "Voicu", "User");
        await EnsureUserAsync(userManager, Guid.Parse("a1000000-0000-0000-0000-000000000013"), "bogdan.petrescu@demo.local", DemoUserPassword, "Bogdan", "Petrescu", "User");
        await EnsureUserAsync(userManager, Guid.Parse("a1000000-0000-0000-0000-000000000014"), "irina.serban@demo.local", DemoUserPassword, "Irina", "Serban", "User");
    }

    private static async Task EnsureAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        Guid userId,
        string email,
        string password,
        string firstName,
        string lastName)
        => await EnsureUserAsync(userManager, userId, email, password, firstName, lastName, "Admin");

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        Guid userId,
        string email,
        string password,
        string firstName,
        string lastName,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                Id = userId,
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
                throw new InvalidOperationException($"Failed to seed user '{email}': {string.Join(", ", createResult.Errors.Select(x => x.Description))}");
        }
        else
        {
            user.FirstName = firstName;
            user.LastName = lastName;
            user.UserName = email;
            user.EmailConfirmed = true;
            user.IsActive = true;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                throw new InvalidOperationException($"Failed to update user '{email}': {string.Join(", ", updateResult.Errors.Select(x => x.Description))}");

            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await userManager.ResetPasswordAsync(user, resetToken, password);
            if (!passwordResult.Succeeded)
                throw new InvalidOperationException($"Failed to reset password for user '{email}': {string.Join(", ", passwordResult.Errors.Select(x => x.Description))}");
        }

        if (!await userManager.IsInRoleAsync(user, role))
            await userManager.AddToRoleAsync(user, role);
    }
}
