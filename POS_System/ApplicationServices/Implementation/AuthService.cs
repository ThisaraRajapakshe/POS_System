using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using POS_System.ApplicationServices;
using POS_System.Data;
using POS_System.Models.Dto;
using POS_System.Models.Identity;

namespace POS_System.ApplicationServices.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly PosSystemAuthDbContext _authDb;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ITokenService tokenService,
            PosSystemAuthDbContext authDb,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _authDb = authDb;
            _logger = logger;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(loginDto.Username) 
                          ?? await _userManager.FindByEmailAsync(loginDto.Username);

                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("Login attempt failed for username: {Username}", loginDto.Username);
                    return null;
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);
                
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Password check failed for user: {UserId}", user.Id);
                    return null;
                }

                // Update last login timestamp
                user.LastLoginAt = DateTimeOffset.UtcNow;
                await _userManager.UpdateAsync(user);

                var authResponse = await _tokenService.GenerateTokensAsync(user);
                
                _logger.LogInformation("User {UserId} logged in successfully", user.Id);
                return authResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}", loginDto.Username);
                return null;
            }
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByNameAsync(registerDto.Username) 
                                 ?? await _userManager.FindByEmailAsync(registerDto.Email);
                
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration attempt with existing username/email: {Username}/{Email}", 
                        registerDto.Username, registerDto.Email);
                    return null;
                }

                // Validate role if provided
                if (!string.IsNullOrEmpty(registerDto.Role) && !await _roleManager.RoleExistsAsync(registerDto.Role))
                {
                    _logger.LogWarning("Registration attempt with invalid role: {Role}", registerDto.Role);
                    return null;
                }

                var user = new ApplicationUser
                {
                    UserName = registerDto.Username,
                    Email = registerDto.Email,
                    FullName = registerDto.FullName,
                    BranchId = registerDto.BranchId,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);
                
                if (!result.Succeeded)
                {
                    _logger.LogWarning("User creation failed for {Username}: {Errors}", 
                        registerDto.Username, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return null;
                }

                // Assign role if provided, otherwise assign default Cashier role
                var roleToAssign = !string.IsNullOrEmpty(registerDto.Role) ? registerDto.Role : RoleConstants.Cashier;
                await _userManager.AddToRoleAsync(user, roleToAssign);

                var authResponse = await _tokenService.GenerateTokensAsync(user);
                
                _logger.LogInformation("User {UserId} registered successfully with role {Role}", user.Id, roleToAssign);
                return authResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for username: {Username}", registerDto.Username);
                return null;
            }
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(RefreshRequestDto refreshRequest)
        {
            try
            {
                var result = await _tokenService.RefreshTokenAsync(refreshRequest);
                
                if (result != null)
                {
                    _logger.LogInformation("Token refreshed successfully");
                }
                else
                {
                    _logger.LogWarning("Token refresh failed");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return null;
            }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            try
            {
                var result = await _tokenService.RevokeRefreshTokenAsync(refreshToken);
                
                if (result)
                {
                    _logger.LogInformation("Refresh token revoked successfully");
                }
                else
                {
                    _logger.LogWarning("Failed to revoke refresh token");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token");
                return false;
            }
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            try
            {
                // Revoke all refresh tokens for the user
                var userTokens = await _authDb.RefreshTokens
                    .Where(rt => rt.UserId == userId && !rt.Revoked)
                    .Select(rt => rt.Token)
                    .ToListAsync();

                if (userTokens.Any())
                {
                    await _tokenService.RevokeBatchAsync(userTokens);
                }

                _logger.LogInformation("User {UserId} logged out successfully", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !user.IsActive)
                {
                    return false;
                }

                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
                    return true;
                }
                
                _logger.LogWarning("Password change failed for user {UserId}: {Errors}", 
                    userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            try
            {
                return await _userManager.FindByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user: {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> AssignRoleAsync(string userId, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || !await _roleManager.RoleExistsAsync(role))
                {
                    return false;
                }

                var result = await _userManager.AddToRoleAsync(user, role);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("Role {Role} assigned to user {UserId}", role, userId);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {Role} to user {UserId}", role, userId);
                return false;
            }
        }

        public async Task<bool> RemoveRoleAsync(string userId, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                var result = await _userManager.RemoveFromRoleAsync(user, role);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("Role {Role} removed from user {UserId}", role, userId);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role {Role} from user {UserId}", role, userId);
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Enumerable.Empty<string>();
                }

                return await _userManager.GetRolesAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles for user: {UserId}", userId);
                return Enumerable.Empty<string>();
            }
        }
    }
}
