using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Experimental;
using POS_System.Configurations;
using POS_System.Data;
using POS_System.Models.Dto;
using POS_System.Models.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
            _key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

        }

        public async Task<AuthResponseDto> GenerateTokensAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var now = DateTime.UtcNow;
            var expiry = now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

            // Generate JWT Claims
            var jti = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? ""),
                new Claim("nameid", user.Id),                     // short name identifier
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.FullName ?? user.UserName ?? "")
            };

            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            if (!string.IsNullOrEmpty(user.BranchId))
                claims.Add(new Claim("branchId", user.BranchId));

            foreach (var r in roles)
                claims.Add(new Claim("role", r));



            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiry,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            // Create Refresh Token
            var refreshToken = GenerateRandomTokenString();
            var refreshEntity = new RefreshToken
            {
                Token = refreshToken,
                JwtId = jti,
                CreationDate = now,
                ExpiryDate = now.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                UserId = user.Id,
                Used = false,
                Revoked = false
            };
            _authDb.RefreshTokens.Add(refreshEntity);
            await _authDb.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiry,
                Roles = roles
            };

        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(RefreshRequestDto request)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // get principal from expired token (don't validate lifetime)
            var principal = GetPrincipalFromToken(request.AccessToken, validateLifetime: false);
            if (principal == null) return null;

            var jwtId = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var userId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(jwtId) || string.IsNullOrEmpty(userId)) return null;

            var stored = await _authDb.RefreshTokens.FirstOrDefaultAsync(x => x.Token == request.RefreshToken);
            if (stored == null) return null;

            // Ensure stoted token belongs to user and matches jwt Id
            if (stored.UserId != userId || stored.JwtId != jwtId) return null;
            if (stored.ExpiryDate < DateTime.UtcNow || stored.Used || stored.Revoked) return null;

            // Mark used & revoked to prevent reuse (rotation)
            stored.Used = true;
            stored.Revoked = true;
            _authDb.RefreshTokens.Update(stored);
            await _authDb.SaveChangesAsync();

            // issue new tokens
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return await GenerateTokensAsync(user);



        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            var stored = await _authDb.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);
            if (stored == null) return false;
            stored.Revoked = true;
            _authDb.RefreshTokens.Update(stored);
            try
            {
                await _authDb.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrent modification
                return false;
            }
            catch (DbUpdateException)
            {
                // Handle database constraints/errors
                return false;
            }
        }

        // batch revocation for mass token invalidation
        public async Task<int> RevokeBatchAsync(IEnumerable<string> tokens)
        {
            var tokensToRevoke = await _authDb.RefreshTokens
                .Where(x => tokens.Contains(x.Token) && !x.Revoked)
                .ToListAsync();

            tokensToRevoke.ForEach(t => t.Revoked = true);
            return await _authDb.SaveChangesAsync();
        }
        private string GenerateRandomTokenString()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromToken(string token, bool validateLifetime)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(_key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = validateLifetime, // false to allow reading expired token
                    ClockSkew = TimeSpan.Zero
                };
                var principal = tokenHandler.ValidateToken(token, parameters, out var validatedToken);

                // extra check: token alg
                if (validatedToken is JwtSecurityToken jwt && jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    return principal;

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
