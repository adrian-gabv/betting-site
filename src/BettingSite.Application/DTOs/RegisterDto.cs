using System.ComponentModel.DataAnnotations;

namespace BettingSite.Application.DTOs
{
    public class RegisterDto
    {
        public required string Username { get; set; }

        [StringLength(64, MinimumLength = 8)]
        public required string Password { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }

        [EmailAddress]
        public required string Email { get; set; }
    }
}
