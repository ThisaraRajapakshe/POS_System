namespace POS_System.Models.Dto
{
    public class RegisterDto
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? FullName { get; set; }
        public string? BranchId { get; set; }
        public string? Role { get; set; }
    }
}
