using Microsoft.AspNetCore.Identity;

namespace BettingSite.Infrastructure.Identity
{
    public class ApplicationRole : IdentityRole<int>
    {
        public ICollection<ApplicationUserRole> UserRoles { get; set; } = [];
    }
}
