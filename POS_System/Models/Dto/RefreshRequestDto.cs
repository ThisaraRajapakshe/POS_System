namespace POS_System.Models.Dto
{
    public class RefreshRequestDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
