namespace BettingSite.Application.DTOs
{
    public class MemberDto
    {
        public int Id { get; set; }
        public required string UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public decimal Money { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public PhotoDto? Avatar { get; set; }
    }
}
