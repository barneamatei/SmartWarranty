using IdentityManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace IdentityManagement.Infrastructure.Data;

public static class DbInitializer
{
    private static readonly string[] Roles = ["Admin", "User", "SupportAgent"];

    public static async Task InitializeAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                if (!result.Succeeded)
                    throw new InvalidOperationException($"Failed to create role '{role}': {string.Join(", ", result.Errors.Select(error => error.Description))}");
            }
        }
    }
}
