using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class UserDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Token { get; set; }

        public string PhotoUrl { get; set; }

        public int Money { get; set; }
    }
}
