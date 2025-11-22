using Microsoft.AspNetCore.Identity;

namespace POS_System.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        // UI / reporting
        public string? FullName { get; set; }

        public string? EmployeeId { get; set; }

        // Branch scoping
        public string? BranchId { get; set; }

        public string? BranchName { get; set; }

        // Soft disable + audit info
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? LastLoginAt { get; set; }
    }
}
