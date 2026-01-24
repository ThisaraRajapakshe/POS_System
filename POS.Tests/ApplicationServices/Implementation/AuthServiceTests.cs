using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using POS_System.ApplicationServices.Implementation;
using POS_System.ApplicationServices;
using POS_System.Models.Identity;
using POS_System.Models.Dto;
using POS_System.Data;

namespace POS.Tests.ApplicationServices.Implementation
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly PosSystemAuthDbContext _dbContext;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            // --- BOILERPLATE: Identity Setup ---
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object, contextAccessorMock.Object, claimsFactoryMock.Object, null, null, null, null);

            var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
            _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
                roleStoreMock.Object, null, null, null, null);
            // --- END BOILERPLATE ---

            _tokenServiceMock = new Mock<ITokenService>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            // --- Setup In-Memory Database ---
            var options = new DbContextOptionsBuilder<PosSystemAuthDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new PosSystemAuthDbContext(options);

            _service = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _roleManagerMock.Object,
                _tokenServiceMock.Object,
                _dbContext,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsValid()
        {
            var loginDto = new LoginDto { Username = "admin", Password = "Password123!" };
            var user = new ApplicationUser { Id = "U1", UserName = "admin", IsActive = true };
            var authResponse = new AuthResponseDto { AccessToken = "valid-token" };

            _userManagerMock.Setup(u => u.FindByNameAsync(loginDto.Username)).ReturnsAsync(user);
            _signInManagerMock.Setup(s => s.CheckPasswordSignInAsync(user, loginDto.Password, true)).ReturnsAsync(SignInResult.Success);
            _tokenServiceMock.Setup(t => t.GenerateTokensAsync(user)).ReturnsAsync(authResponse);

            var result = await _service.LoginAsync(loginDto);

            Assert.NotNull(result);
            Assert.Equal("valid-token", result.AccessToken);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenUserNotFound()
        {
            var loginDto = new LoginDto { Username = "ghost" };
            _userManagerMock.Setup(u => u.FindByNameAsync(loginDto.Username)).ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(u => u.FindByEmailAsync(loginDto.Username)).ReturnsAsync((ApplicationUser?)null);

            var result = await _service.LoginAsync(loginDto);

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenPasswordInvalid()
        {
            var loginDto = new LoginDto { Username = "admin", Password = "WrongPassword" };
            var user = new ApplicationUser { UserName = "admin", IsActive = true };

            _userManagerMock.Setup(u => u.FindByNameAsync(loginDto.Username)).ReturnsAsync(user);
            _signInManagerMock.Setup(s => s.CheckPasswordSignInAsync(user, loginDto.Password, true)).ReturnsAsync(SignInResult.Failed);

            var result = await _service.LoginAsync(loginDto);

            Assert.Null(result);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnToken_WhenSuccess()
        {
            var regDto = new RegisterDto { Username = "newuser", Email = "new@test.com", Password = "Pass", Role = "Cashier" };
            var authResponse = new AuthResponseDto { AccessToken = "new-token" };

            _userManagerMock.Setup(u => u.FindByNameAsync(regDto.Username)).ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(u => u.FindByEmailAsync(regDto.Email)).ReturnsAsync((ApplicationUser?)null);
            _roleManagerMock.Setup(r => r.RoleExistsAsync(regDto.Role)).ReturnsAsync(true);
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), regDto.Password)).ReturnsAsync(IdentityResult.Success);
            _tokenServiceMock.Setup(t => t.GenerateTokensAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(authResponse);

            var result = await _service.RegisterAsync(regDto);

            Assert.NotNull(result);
            Assert.Equal("new-token", result.AccessToken);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnNull_WhenUserAlreadyExists()
        {
            var regDto = new RegisterDto { Username = "existing" };
            _userManagerMock.Setup(u => u.FindByNameAsync(regDto.Username)).ReturnsAsync(new ApplicationUser());

            var result = await _service.RegisterAsync(regDto);

            Assert.Null(result);
        }

        [Fact]
        public async Task LogoutAsync_ShouldRevokeTokens_WhenUserExists()
        {
            // ARRANGE
            var userId = "User1";
            var token1 = "refresh-token-1";
            var token2 = "refresh-token-2";

            // Seed the In-Memory Database
            // FIX: Added 'JwtId' and 'ExpiryDate' to satisfy database requirements
            _dbContext.RefreshTokens.AddRange(
                new RefreshToken
                {
                    JwtId = "j1",
                    UserId = userId,
                    Token = token1,
                    Revoked = false,
                    ExpiryDate = DateTime.UtcNow.AddDays(1),
                    CreationDate = DateTime.UtcNow
                },
                new RefreshToken
                {
                    JwtId = "j2",
                    UserId = userId,
                    Token = token2,
                    Revoked = false,
                    ExpiryDate = DateTime.UtcNow.AddDays(1),
                    CreationDate = DateTime.UtcNow
                },
                new RefreshToken
                {
                    JwtId = "j3",
                    UserId = "OtherUser",
                    Token = "other-token",
                    Revoked = false,
                    ExpiryDate = DateTime.UtcNow.AddDays(1),
                    CreationDate = DateTime.UtcNow
                }
            );
            await _dbContext.SaveChangesAsync();

            // ACT
            var result = await _service.LogoutAsync(userId);

            // ASSERT
            Assert.True(result);
            _tokenServiceMock.Verify(t => t.RevokeBatchAsync(It.Is<List<string>>(tokens =>
                tokens.Contains(token1) &&
                tokens.Contains(token2) &&
                tokens.Count == 2
            )), Times.Once);
        }
    }
}