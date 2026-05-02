using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class MemberDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }
        public string AvatarUrl { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public int Money { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public PhotoDto Avatar { get; set; }
    }
}
