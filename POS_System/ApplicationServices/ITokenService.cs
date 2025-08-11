using POS_System.Models.Dto;
using POS_System.Models.Identity;

namespace POS_System.ApplicationServices
{
    public interface ITokenService
    {
        Task<AuthResponseDto> GenerateTokensAsync(ApplicationUser user);
        Task<AuthResponseDto?> RefreshTokenAsync(RefreshRequestDto request);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
        Task<int> RevokeBatchAsync(IEnumerable<string> tokens);
    }
}
