using POS_System.Models.Dto;
using POS_System.Models.Identity;

namespace POS_System.ApplicationServices
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto?> RefreshTokenAsync(RefreshRequestDto refreshRequest);
        Task<bool> RevokeTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(string userId);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<bool> AssignRoleAsync(string userId, string role);
        Task<bool> RemoveRoleAsync(string userId, string role);
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);
    }
}
