using Microsoft.AspNetCore.Identity;

namespace BettingSite.Infrastructure.Identity
{
    public class ApplicationUserRole : IdentityUserRole<int>
    {
        public ApplicationUser? User { get; set; }
        public ApplicationRole? Role { get; set; }
    }
}
