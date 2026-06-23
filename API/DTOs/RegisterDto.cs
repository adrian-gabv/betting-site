using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegisterDto
    {
        public required string Username { get; set; }

        [StringLength(15, MinimumLength = 6)]
        public required string Password { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }

        [EmailAddress]
        public required string Email { get; set; }
    }
}
