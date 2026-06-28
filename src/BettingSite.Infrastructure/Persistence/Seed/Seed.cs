using System.Text.Json;
using BettingSite.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BettingSite.Infrastructure.Persistence.Seed
{
    public class Seed
    {
        public static async Task SeedRoles(RoleManager<ApplicationRole> roleManager)
        {
            string[] roles = ["User", "Moderator", "Admin"];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new ApplicationRole { Name = role });
            }
        }

        public static async Task SeedUsers(UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            if (await userManager.Users.AnyAsync()) return;

            var adminUsername = config["SeedSettings:AdminUsername"]
                ?? throw new InvalidOperationException("SeedSettings:AdminUsername is not configured");
            var adminEmail = config["SeedSettings:AdminEmail"]
                ?? throw new InvalidOperationException("SeedSettings:AdminEmail is not configured");
            var adminPassword = config["SeedSettings:AdminPassword"]
                ?? throw new InvalidOperationException("SeedSettings:AdminPassword is not configured");
            var defaultUserPassword = config["SeedSettings:DefaultUserPassword"]
                ?? throw new InvalidOperationException("SeedSettings:DefaultUserPassword is not configured");

            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<ApplicationUser>>(userData) ?? [];

            foreach (var user in users)
            {
                user.UserName = user.UserName?.ToLower();
                await userManager.CreateAsync(user, defaultUserPassword);
                await userManager.AddToRoleAsync(user, "User");
            }

            var admin = new ApplicationUser { UserName = adminUsername, Email = adminEmail };
            await userManager.CreateAsync(admin, adminPassword);
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
