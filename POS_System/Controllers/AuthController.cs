using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS_System.ApplicationServices;
using POS_System.Models.Dto;
using POS_System.Models.Identity;
using System.Security.Claims;

namespace POS_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(loginDto);
            
            if (result == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            return Ok(result);
        }

        [HttpPost("register")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme ,Roles = $"{RoleConstants.Admin},{RoleConstants.Manager}")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(registerDto);
            
            if (result == null)
            {
                return BadRequest(new { message = "Registration failed. User may already exist or invalid data provided." });
            }

            return CreatedAtAction(nameof(GetProfile), new { }, result);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshRequestDto refreshRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RefreshTokenAsync(refreshRequest);
            
            if (result == null)
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }

            return Ok(result);
        }

        [HttpPost("revoke")]
        [Authorize (AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto revokeTokenDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RevokeTokenAsync(revokeTokenDto.RefreshToken);
            
            if (!result)
            {
                return BadRequest(new { message = "Failed to revoke token" });
            }

            return Ok(new { message = "Token revoked successfully" });
        }

        [HttpPost("logout")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "Invalid user context" });
            }

            var result = await _authService.LogoutAsync(userId);
            
            if (!result)
            {
                return BadRequest(new { message = "Logout failed" });
            }

            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("change-password")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "Invalid user context" });
            }

            var result = await _authService.ChangePasswordAsync(userId, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            
            if (!result)
            {
                return BadRequest(new { message = "Password change failed. Please check your current password." });
            }

            return Ok(new { message = "Password changed successfully" });
        }

        [HttpGet("profile")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "Invalid user context" });
            }

            var user = await _authService.GetUserByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var roles = await _authService.GetUserRolesAsync(userId);

            var profileDto = new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                BranchId = user.BranchId ?? string.Empty,
                BranchName = user.BranchName ?? string.Empty,
                EmployeeId = user.EmployeeId,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = [.. roles ?? []]
            };

            return Ok(profileDto);
        }

        [HttpPost("assign-role")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{RoleConstants.Admin},{RoleConstants.Manager}")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto assignRoleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.AssignRoleAsync(assignRoleDto.UserId, assignRoleDto.Role);
            
            if (!result)
            {
                return BadRequest(new { message = "Failed to assign role. User or role may not exist." });
            }

            return Ok(new { message = $"Role '{assignRoleDto.Role}' assigned successfully" });
        }

        [HttpPost("remove-role")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{RoleConstants.Admin},{RoleConstants.Manager}")]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto removeRoleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RemoveRoleAsync(removeRoleDto.UserId, removeRoleDto.Role);
            
            if (!result)
            {
                return BadRequest(new { message = "Failed to remove role" });
            }

            return Ok(new { message = $"Role '{removeRoleDto.Role}' removed successfully" });
        }

        [HttpGet("users/{userId}/roles")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{RoleConstants.Admin},{RoleConstants.Manager}")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var roles = await _authService.GetUserRolesAsync(userId);
            return Ok(new { UserId = userId, Roles = roles });
        }
    }
}
