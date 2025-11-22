namespace POS_System.Models.Dto
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public IEnumerable<string>? Roles { get; set; }
    }
}
