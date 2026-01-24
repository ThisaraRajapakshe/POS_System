using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using POS_System.Controllers;
using POS_System.ApplicationServices;
using POS_System.Models.Dto;
using POS_System.Models.Identity;

namespace POS.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _serviceMock;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _serviceMock = new Mock<IAuthService>();
            _loggerMock = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_serviceMock.Object, _loggerMock.Object);
        }

        // --- HELPER: Mock the logged-in user ---
        private void SetUserContext(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        // --- LOGIN TESTS ---

        [Fact]
        public async Task Login_ShouldReturnOk_WhenSuccess()
        {
            // ARRANGE
            var loginDto = new LoginDto { Username = "admin", Password = "pwd" };
            var response = new AuthResponseDto { AccessToken = "token" };
            _serviceMock.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync(response);

            // ACT
            var result = await _controller.Login(loginDto);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenFailed()
        {
            // ARRANGE
            var loginDto = new LoginDto { Username = "admin", Password = "pwd" };
            _serviceMock.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync((AuthResponseDto?)null);

            // ACT
            var result = await _controller.Login(loginDto);

            // ASSERT
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // --- REGISTER TESTS ---

        [Fact]
        public async Task Register_ShouldReturnCreated_WhenSuccess()
        {
            // ARRANGE
            var regDto = new RegisterDto { Username = "new", Password = "pwd" };
            var response = new AuthResponseDto { AccessToken = "token" };
            _serviceMock.Setup(s => s.RegisterAsync(regDto)).ReturnsAsync(response);

            // ACT
            var result = await _controller.Register(regDto);

            // ASSERT
            var createdResult = Assert.IsType<CreatedAtActionResult>(result); // CreatedAtAction
            Assert.Equal(response, createdResult.Value);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenFailed()
        {
            // ARRANGE
            var regDto = new RegisterDto { Username = "exists" };
            _serviceMock.Setup(s => s.RegisterAsync(regDto)).ReturnsAsync((AuthResponseDto?)null);

            // ACT
            var result = await _controller.Register(regDto);

            // ASSERT
            var badReq = Assert.IsType<BadRequestObjectResult>(result);
        }

        // --- REFRESH TOKEN TESTS ---

        [Fact]
        public async Task RefreshToken_ShouldReturnOk_WhenSuccess()
        {
            // ARRANGE
            var req = new RefreshRequestDto { AccessToken = "a", RefreshToken = "b" };
            var response = new AuthResponseDto { AccessToken = "new_a" };
            _serviceMock.Setup(s => s.RefreshTokenAsync(req)).ReturnsAsync(response);

            // ACT
            var result = await _controller.RefreshToken(req);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnUnauthorized_WhenFailed()
        {
            // ARRANGE
            var req = new RefreshRequestDto { AccessToken = "a", RefreshToken = "b" };
            _serviceMock.Setup(s => s.RefreshTokenAsync(req)).ReturnsAsync((AuthResponseDto?)null);

            // ACT
            var result = await _controller.RefreshToken(req);

            // ASSERT
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // --- LOGOUT TESTS (Uses User Context) ---

        [Fact]
        public async Task Logout_ShouldReturnOk_WhenSuccess()
        {
            // ARRANGE
            var userId = "U1";
            SetUserContext(userId); // Fake the login
            _serviceMock.Setup(s => s.LogoutAsync(userId)).ReturnsAsync(true);

            // ACT
            var result = await _controller.Logout();

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Logout_ShouldReturnBadRequest_WhenServiceFails()
        {
            // ARRANGE
            var userId = "U1";
            SetUserContext(userId);
            _serviceMock.Setup(s => s.LogoutAsync(userId)).ReturnsAsync(false);

            // ACT
            var result = await _controller.Logout();

            // ASSERT
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Logout_ShouldReturnBadRequest_WhenUserContextMissing()
        {
            // ARRANGE
            // Do NOT call SetUserContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() } // Empty user
            };

            // ACT
            var result = await _controller.Logout();

            // ASSERT
            var badReq = Assert.IsType<BadRequestObjectResult>(result);
        }

        // --- GET PROFILE TESTS ---

        [Fact]
        public async Task GetProfile_ShouldReturnDto_WhenFound()
        {
            // ARRANGE
            var userId = "U1";
            SetUserContext(userId);

            var domainUser = new ApplicationUser
            {
                Id = userId,
                UserName = "TestUser",
                Email = "test@test.com",
                FullName = "Test Person",
                IsActive = true
            };
            var roles = new List<string> { "Admin" };

            _serviceMock.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(domainUser);
            _serviceMock.Setup(s => s.GetUserRolesAsync(userId)).ReturnsAsync(roles);

            // ACT
            var result = await _controller.GetProfile();

            // ASSERT
            var actionResult = Assert.IsType<OkObjectResult>(result.Result); // Extract from ActionResult<T>
            var dto = Assert.IsType<UserProfileDto>(actionResult.Value);

            Assert.Equal("TestUser", dto.UserName);
            Assert.Single(dto.Roles);
            Assert.Equal("Admin", dto.Roles.First());
        }

        [Fact]
        public async Task GetProfile_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // ARRANGE
            var userId = "U1";
            SetUserContext(userId);
            _serviceMock.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync((ApplicationUser?)null);

            // ACT
            var result = await _controller.GetProfile();

            // ASSERT
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        // --- CHANGE PASSWORD TESTS ---

        [Fact]
        public async Task ChangePassword_ShouldReturnOk_WhenSuccess()
        {
            // ARRANGE
            var userId = "U1";
            SetUserContext(userId);
            var dto = new ChangePasswordDto { CurrentPassword = "old", NewPassword = "new" };

            _serviceMock.Setup(s => s.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword))
                        .ReturnsAsync(true);

            // ACT
            var result = await _controller.ChangePassword(dto);

            // ASSERT
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnBadRequest_WhenFailed()
        {
            // ARRANGE
            var userId = "U1";
            SetUserContext(userId);
            var dto = new ChangePasswordDto { CurrentPassword = "old", NewPassword = "new" };

            _serviceMock.Setup(s => s.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword))
                        .ReturnsAsync(false);

            // ACT
            var result = await _controller.ChangePassword(dto);

            // ASSERT
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}