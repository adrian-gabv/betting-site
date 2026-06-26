using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedRoles(RoleManager<AppRole> roleManager)
        {
            string[] roles = ["User", "Moderator", "Admin"];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new AppRole { Name = role });
            }
        }

        public static async Task SeedUsers(UserManager<AppUser> userManager, IConfiguration config)
        {
            if (await userManager.Users.AnyAsync()) return;

            var adminUsername = config["SeedSettings:AdminUsername"]
                ?? throw new InvalidOperationException("SeedSettings:AdminUsername is not configured");
            var adminPassword = config["SeedSettings:AdminPassword"]
                ?? throw new InvalidOperationException("SeedSettings:AdminPassword is not configured");
            var defaultUserPassword = config["SeedSettings:DefaultUserPassword"]
                ?? throw new InvalidOperationException("SeedSettings:DefaultUserPassword is not configured");

            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData) ?? [];

            foreach (var user in users)
            {
                user.UserName = user.UserName?.ToLower();
                await userManager.CreateAsync(user, defaultUserPassword);
                await userManager.AddToRoleAsync(user, "User");
            }

            var admin = new AppUser { UserName = adminUsername };
            await userManager.CreateAsync(admin, adminPassword);
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
