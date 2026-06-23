namespace API.DTOs
{
    public class PhotoDto
    {
        public int Id { get; set; }
        public required string Url { get; set; }
        public string? PublicId { get; set; }
    }
}