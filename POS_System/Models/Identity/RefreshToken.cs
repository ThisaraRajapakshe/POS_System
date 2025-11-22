namespace POS_System.Models.Identity
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = null!;
        public string JwtId { get; set; } = null!;      // jti of the access token it was issued for
        public DateTime CreationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool Used { get; set; } = false;
        public bool Revoked { get; set; } = false;
        public string UserId { get; set; } = null!;
    }
}
