using System.ComponentModel.DataAnnotations;

namespace POS_System.Models.Dto
{
    public class RevokeTokenDto
    {
        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}
