using System.ComponentModel.DataAnnotations;

namespace POS_System.Models.Dto
{
    public class AssignRoleDto
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public string Role { get; set; } = null!;
    }
}
