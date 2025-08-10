using POS_System.Models.Dto;
using POS_System.Models.Identity;

namespace POS_System.ApplicationServices
{
    public interface ITokenService
    {
        Task<AuthResponseDto> GenerateTokensDto(ApplicationUser user);
        Task<AuthResponseDto?> RefreshTokenAsync(RefreshRequestDto request);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    }
}
