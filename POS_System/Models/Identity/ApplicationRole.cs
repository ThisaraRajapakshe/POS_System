using Microsoft.AspNetCore.Identity;

namespace POS_System.Models.Identity
{
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }
    }
}
