using Xunit;
using Moq;
using AutoMapper;
using POS_System.ApplicationServices.Implementation;
using POS_System.Models.Domain;
using POS_System.Models.Dto;
using POS_System.Repositories;

namespace POS.Tests.ApplicationServices.Implementation
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _repoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly OrderServise _service; // Matches your class name

        public OrderServiceTests()
        {
            _repoMock = new Mock<IOrderRepository>();
            _mapperMock = new Mock<IMapper>();
            _service = new OrderServise(_mapperMock.Object, _repoMock.Object);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldProcessLogic_AndReturnDto()
        {
            // ARRANGE
            var userId = "User123";
            var cashierName = "John Doe";

            // 1. Input DTO (Updated to use OrderItemDto)
            var createDto = new CreateOrderDto
            {
                IsPending = false, // This should set Status to "Completed"
                OrderItems = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        ProductLineItemId = "PL1", // Matches your DTO
                        Quantity = 1,
                        SalesPrice = 100
                    }
                }
            };

            // 2. The Domain object the Mapper creates (initially empty of IDs)
            var initialDomain = new Order
            {
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductLineItemId = "PL1", SalesPrice = 100 }
                }
            };

            // 3. The Saved Domain object (simulating what DB returns)
            var savedDomain = new Order
            {
                Id = "ORDER-1",
                UserId = userId,
                Status = "Completed",
                OrderNumber = "INV-2025-ABCD"
            };

            // 4. The Final Response DTO
            var responseDto = new OrderResponseDto
            {
                Id = "ORDER-1",
                Status = "Completed",
                OrderNumber = "INV-2025-ABCD"
            };

            // Setup Mocks
            _mapperMock.Setup(m => m.Map<Order>(createDto)).Returns(initialDomain);

            // Critical: Setup Repo to return the saved order when called
            _repoMock.Setup(r => r.CreateOrderAsync(It.IsAny<Order>()))
                     .ReturnsAsync(savedDomain);

            _mapperMock.Setup(m => m.Map<OrderResponseDto>(savedDomain)).Returns(responseDto);

            // ACT
            var result = await _service.CreateOrderAsync(createDto, userId, cashierName);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("Completed", result.Status);
            Assert.Equal("ORDER-1", result.Id);

            // VERIFY LOGIC: Did the service actually set the fields correctly?
            _repoMock.Verify(r => r.CreateOrderAsync(It.Is<Order>(o =>
                o.UserId == userId &&
                o.CashierName == cashierName &&
                o.Status == "Completed" &&
                !string.IsNullOrEmpty(o.OrderNumber) // Verify OrderNumber was generated
            )), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldSetPendingStatus_WhenIsPendingTrue()
        {
            // ARRANGE
            var userId = "User1";
            var cashierName = "Cashier1";
            var createDto = new CreateOrderDto
            {
                IsPending = true, // Should result in "Pending"
                OrderItems = new List<OrderItemDto>()
            };

            var initialDomain = new Order { OrderItems = new List<OrderItem>() };
            var savedDomain = new Order { Status = "Pending" };
            var responseDto = new OrderResponseDto { Status = "Pending" };

            _mapperMock.Setup(m => m.Map<Order>(createDto)).Returns(initialDomain);
            _repoMock.Setup(r => r.CreateOrderAsync(It.IsAny<Order>())).ReturnsAsync(savedDomain);
            _mapperMock.Setup(m => m.Map<OrderResponseDto>(savedDomain)).Returns(responseDto);

            // ACT
            await _service.CreateOrderAsync(createDto, userId, cashierName);

            // ASSERT
            // Verify logic specifically checks for "Pending"
            _repoMock.Verify(r => r.CreateOrderAsync(It.Is<Order>(o => o.Status == "Pending")), Times.Once);
        }

        [Fact]
        public async Task GetOrdersAsync_ShouldReturnList()
        {
            // ARRANGE
            var domainList = new List<Order> { new Order { Id = "O1" } };
            var dtoList = new List<OrderResponseDto> { new OrderResponseDto { Id = "O1" } };

            _repoMock.Setup(r => r.GetAllOrdersAsync()).ReturnsAsync(domainList);
            _mapperMock.Setup(m => m.Map<List<OrderResponseDto>>(domainList)).Returns(dtoList);

            // ACT
            var result = await _service.GetOrdersAsync();

            // ASSERT
            Assert.Single(result);
            _repoMock.Verify(r => r.GetAllOrdersAsync(), Times.Once);
        }
    }
}