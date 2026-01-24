using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // Needed for HttpContext
using System.Security.Claims;    // Needed for ClaimsPrincipal
using POS_System.Controllers;
using POS_System.ApplicationServices;
using POS_System.Models.Dto;

namespace POS.Tests.Controllers
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderService> _serviceMock;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _serviceMock = new Mock<IOrderService>();
            _controller = new OrdersController(_serviceMock.Object);
        }

        // --- HELPER: This creates a "fake" logged-in user ---
        private void SetUserContext(string userId, string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userName)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetOrders_ShouldReturnOk_WithList()
        {
            // ARRANGE
            var dtoList = new List<OrderResponseDto>
            {
                new OrderResponseDto { Id = "O1", OrderNumber = "INV-001" }
            };

            _serviceMock.Setup(s => s.GetOrdersAsync()).ReturnsAsync(dtoList);

            // ACT
            var result = await _controller.GetOrders();

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<OrderResponseDto>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task CreateOrder_ShouldReturnOk_WhenUserIsLoggedIn()
        {
            // ARRANGE
            var userId = "U1";
            var userName = "CashierJohn";

            // 1. FAKE THE LOGIN (Critical Step!)
            SetUserContext(userId, userName);

            var request = new CreateOrderDto { TotalAmount = 100 };
            var response = new OrderResponseDto { Id = "O1", Status = "Pending" };

            // 2. Setup Service to expect the exact UserId and Name we faked
            _serviceMock.Setup(s => s.CreateOrderAsync(request, userId, userName))
                        .ReturnsAsync(response);

            // ACT
            var result = await _controller.CreateOrder(request);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<OrderResponseDto>(okResult.Value);
            Assert.Equal("O1", returnValue.Id);
        }

        [Fact]
        public async Task CreateOrder_ShouldReturnUnauthorized_WhenUserClaimsAreMissing()
        {
            // ARRANGE
            // We set an EMPTY context (User is technically there, but has no ID claim)
            var incompleteUser = new ClaimsPrincipal(new ClaimsIdentity());
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = incompleteUser }
            };

            var request = new CreateOrderDto();

            // ACT
            var result = await _controller.CreateOrder(request);

            // ASSERT
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User Id Not Found in Token", unauthorizedResult.Value);
        }
    }
}