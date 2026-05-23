using BarangayPharmaSystem.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace BarangayPharmaSystem.Data;

/// <summary>
/// Seeds the database with required baseline data on application startup.
/// Safe to run multiple times — checks for existence before inserting.
/// </summary>
public static class DbSeeder
{
    private const string AdminRoleName    = "Admin";
    private const string StaffRoleName    = "Staff";
    private const string PatientRoleName  = "Patient";

    private const string DefaultAdminEmail    = "admin@barangaypharma.local";
    private const string DefaultAdminPassword = "Admin@1234";
    private const string DefaultAdminFullName = "System Administrator";

    /// <summary>
    /// Creates the three application roles and the default Admin user if they do not already exist.
    /// Called once at application startup from Program.cs.
    /// </summary>
    public static async Task SeedAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        await EnsureRolesAsync(roleManager);
        await EnsureAdminUserAsync(userManager);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = [AdminRoleName, StaffRoleName, PatientRoleName];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create role '{role}': {errors}");
                }
            }
        }
    }

    private static async Task EnsureAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        // Skip if the default admin already exists
        var existingAdmin = await userManager.FindByEmailAsync(DefaultAdminEmail);
        if (existingAdmin != null) return;

        var adminUser = new ApplicationUser
        {
            UserName      = DefaultAdminEmail,
            Email         = DefaultAdminEmail,
            FullName      = DefaultAdminFullName,
            EmailConfirmed = true,
            IsDeleted     = false,
            CreatedAt     = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(adminUser, DefaultAdminPassword);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create default admin user: {errors}");
        }

        var roleResult = await userManager.AddToRoleAsync(adminUser, AdminRoleName);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to assign Admin role to default user: {errors}");
        }
    }
}
