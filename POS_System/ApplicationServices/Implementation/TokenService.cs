using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using POS_System.Configurations;
using POS_System.Data;
using POS_System.Models.Dto;
using POS_System.Models.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace POS_System.ApplicationServices.Implementation
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PosSystemAuthDbContext _authDb;
        private readonly JwtSettings _jwtSettings;
        private readonly byte[] _key;

        public TokenService(UserManager<ApplicationUser> userManager,
                            PosSystemAuthDbContext authDb,
                            IOptions<JwtSettings> jwtOptions)
        {
            _userManager = userManager;
            _authDb = authDb;
            _jwtSettings = jwtOptions.Value;
            _key =Encoding.UTF8.GetBytes(_jwtSettings.Key);

        }

        public async Task<AuthResponseDto> GenerateTokensDto(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var now = DateTime.UtcNow;
            var expiry = now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

            // Generate JWT Claims
            var jti = Guid.NewGuid().ToString();
            var claims = new List<Claim> 
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                new Claim(ClaimTypes.Name, user.FullName ?? user.UserName ?? "")
            };
            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            if(!string.IsNullOrEmpty(user.BranchId))
                claims.Add(new Claim("branchId", user.BranchId));

            foreach (var r in roles)
                claims.Add(new Claim(ClaimTypes.Role, r));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {

            }
        }

        public Task<AuthResponseDto?> RefreshTokenAsync(RefreshRequestDto request)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }
    }
}
