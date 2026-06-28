using BettingSite.Domain.Betting;
using Microsoft.AspNetCore.Identity;

namespace BettingSite.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public decimal Money { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime LastActive { get; set; } = DateTime.UtcNow;
    public Photo? Avatar { get; set; }
    public ICollection<ApplicationUserRole> UserRoles { get; set; } = [];
}
