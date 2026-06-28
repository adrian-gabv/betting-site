using BettingSite.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BettingSite.API.Controllers
{
    [Authorize(Policy = "AdminRole")]
    public class AdminController(UserManager<ApplicationUser> userManager) : BaseApiController
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        [HttpGet("get-user-roles")]
        public async Task<ActionResult> GetUserRoles()
        {
            var users = await _userManager.Users
                .Include(ur => ur.UserRoles)
                .ThenInclude(ur => ur.Role)
                .OrderBy(user => user.UserName)
                .Select(user => new
                {
                    user.Id,
                    Username = user.UserName,
                    Roles = user.UserRoles.Where(r => r.Role != null).Select(r => r.Role!.Name).ToList()
                })
                .ToListAsync();
            return Ok(users);
        }

        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            if (string.IsNullOrWhiteSpace(roles)) return BadRequest("You must select at least one role");

            var activeRoles = roles.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var validRoles = new[] { "Admin", "Moderator", "User" };
            if (activeRoles.Except(validRoles).Any()) return BadRequest("Invalid role(s) selected: " + string.Join(", ", activeRoles.Except(validRoles)));

            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return NotFound("User does not exist");

            var userRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.AddToRolesAsync(user, activeRoles.Except(userRoles));
            if (!result.Succeeded) return BadRequest("Failed to add role");

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(activeRoles));
            if (!result.Succeeded) return BadRequest("Failed to remove role");

            return Ok(await _userManager.GetRolesAsync(user));
        }
    }
}
