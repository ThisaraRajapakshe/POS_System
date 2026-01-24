using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using POS_System.ApplicationServices.Implementation;
using POS_System.Models.Identity;
using POS_System.Models.Dto;
using POS_System.Data;
using POS_System.Configurations;
using System.Security.Claims;

namespace POS.Tests.ApplicationServices.Implementation
{
    public class TokenServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IOptions<JwtSettings>> _optionsMock;
        private readonly PosSystemAuthDbContext _dbContext;
        private readonly TokenService _service;

        // We need a specific key for tests (Must be >= 32 chars for HMAC-SHA256)
        private const string TEST_KEY = "SuperSecretKeyForTesting12345678!@#";

        public TokenServiceTests()
        {
            // 1. Setup InMemory Database
            var options = new DbContextOptionsBuilder<PosSystemAuthDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new PosSystemAuthDbContext(options);

            // 2. Setup UserManager Boilerplate
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // 3. Setup JWT Settings
            var jwtSettings = new JwtSettings
            {
                Key = TEST_KEY,
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                AccessTokenExpirationMinutes = 15,
                RefreshTokenExpirationDays = 7
            };
            _optionsMock = new Mock<IOptions<JwtSettings>>();
            _optionsMock.Setup(s => s.Value).Returns(jwtSettings);

            // 4. Inject into Service
            _service = new TokenService(_userManagerMock.Object, _dbContext, _optionsMock.Object);
        }

        [Fact]
        public async Task GenerateTokensAsync_ShouldCreateTokensAndSaveToDb()
        {
            // ARRANGE
            var user = new ApplicationUser { Id = "U1", UserName = "tester", Email = "test@test.com" };

            // Mock Roles
            _userManagerMock.Setup(u => u.GetRolesAsync(user))
                            .ReturnsAsync(new List<string> { "Cashier" });

            // ACT
            var result = await _service.GenerateTokensAsync(user);

            // ASSERT
            Assert.NotNull(result);
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);
            Assert.Contains("Cashier", result.Roles);

            // Verify Refresh Token was saved to DB
            var savedToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == result.RefreshToken);
            Assert.NotNull(savedToken);
            Assert.Equal("U1", savedToken.UserId);
            Assert.False(savedToken.Used);
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnNewTokens_WhenOldTokenIsValid()
        {
            // ARRANGE
            var user = new ApplicationUser { Id = "U1", UserName = "tester" };
            _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string>());
            _userManagerMock.Setup(u => u.FindByIdAsync("U1")).ReturnsAsync(user);

            // 1. Generate an initial valid token pair
            var initialTokens = await _service.GenerateTokensAsync(user);

            // 2. Prepare the Refresh Request
            var request = new RefreshRequestDto
            {
                AccessToken = initialTokens.AccessToken,
                RefreshToken = initialTokens.RefreshToken
            };

            // ACT
            var result = await _service.RefreshTokenAsync(request);

            // ASSERT
            Assert.NotNull(result);
            Assert.NotEqual(initialTokens.AccessToken, result.AccessToken); // Should be new
            Assert.NotEqual(initialTokens.RefreshToken, result.RefreshToken); // Should be new

            // Verify Old Token is marked as Used & Revoked
            var oldTokenEntity = await _dbContext.RefreshTokens.FirstAsync(t => t.Token == initialTokens.RefreshToken);
            Assert.True(oldTokenEntity.Used);
            Assert.True(oldTokenEntity.Revoked);
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnNull_WhenRefreshTokenIsRevoked()
        {
            // ARRANGE
            var user = new ApplicationUser { Id = "U1", UserName = "tester" };
            _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string>());

            // 1. Generate valid tokens
            var initialTokens = await _service.GenerateTokensAsync(user);

            // 2. Manually Revoke the token in DB
            var tokenEntity = await _dbContext.RefreshTokens.FirstAsync(t => t.Token == initialTokens.RefreshToken);
            tokenEntity.Revoked = true;
            await _dbContext.SaveChangesAsync();

            var request = new RefreshRequestDto
            {
                AccessToken = initialTokens.AccessToken,
                RefreshToken = initialTokens.RefreshToken
            };

            // ACT
            var result = await _service.RefreshTokenAsync(request);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task RevokeRefreshTokenAsync_ShouldMarkRevoked_WhenTokenExists()
        {
            // ARRANGE
            var tokenString = "some-random-token";
            _dbContext.RefreshTokens.Add(new RefreshToken
            {
                Token = tokenString,
                JwtId = "j1",
                UserId = "U1",
                Revoked = false,
                // Add required properties for EF Core
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(1),
                Used = false
            });
            await _dbContext.SaveChangesAsync();

            // ACT
            var result = await _service.RevokeRefreshTokenAsync(tokenString);

            // ASSERT
            Assert.True(result);
            var entity = await _dbContext.RefreshTokens.FirstAsync(t => t.Token == tokenString);
            Assert.True(entity.Revoked);
        }

        [Fact]
        public async Task RevokeBatchAsync_ShouldRevokeMultipleTokens()
        {
            // ARRANGE
            var t1 = "token-1";
            var t2 = "token-2";
            _dbContext.RefreshTokens.AddRange(
                new RefreshToken { Token = t1, UserId = "U1", Revoked = false, JwtId = "j1", CreationDate = DateTime.UtcNow, ExpiryDate = DateTime.UtcNow.AddDays(1) },
                new RefreshToken { Token = t2, UserId = "U1", Revoked = false, JwtId = "j2", CreationDate = DateTime.UtcNow, ExpiryDate = DateTime.UtcNow.AddDays(1) }
            );
            await _dbContext.SaveChangesAsync();

            // ACT
            var count = await _service.RevokeBatchAsync(new List<string> { t1, t2 });

            // ASSERT
            Assert.Equal(2, count);
            Assert.True((await _dbContext.RefreshTokens.FirstAsync(t => t.Token == t1)).Revoked);
            Assert.True((await _dbContext.RefreshTokens.FirstAsync(t => t.Token == t2)).Revoked);
        }
    }
}