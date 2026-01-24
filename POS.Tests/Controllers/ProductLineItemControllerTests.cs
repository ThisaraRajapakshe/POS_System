using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using POS_System.Controllers;
using POS_System.ApplicationServices;
using POS_System.Models.Dto;

namespace POS.Tests.Controllers
{
    public class ProductLineItemControllerTests
    {
        private readonly Mock<IProductLineItemService> _serviceMock;
        private readonly ProductLineItemController _controller;

        public ProductLineItemControllerTests()
        {
            _serviceMock = new Mock<IProductLineItemService>();
            _controller = new ProductLineItemController(_serviceMock.Object);
        }

        [Fact]
        public async Task Get_ShouldReturnOk_WithList()
        {
            // ARRANGE
            var dtoList = new List<ProductLineItemDto>
            {
                new ProductLineItemDto { Id = "PL1", BarCodeId = "123" }
            };

            _serviceMock.Setup(s => s.GetProductLineItems()).ReturnsAsync(dtoList);

            // ACT
            var result = await _controller.Get();

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<ProductLineItemDto>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenFound()
        {
            // ARRANGE
            var id = "PL1";
            var dto = new ProductLineItemDto { Id = id, BarCodeId = "123" };

            _serviceMock.Setup(s => s.GetProductLineItem(id)).ReturnsAsync(dto);

            // ACT
            var result = await _controller.Get(id);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ProductLineItemDto>(okResult.Value);
            Assert.Equal("123", returnValue.BarCodeId);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenNull()
        {
            // ARRANGE
            var id = "99";
            _serviceMock.Setup(s => s.GetProductLineItem(id)).ReturnsAsync((ProductLineItemDto?)null);

            // ACT
            var result = await _controller.Get(id);

            // ASSERT
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_ShouldReturnOk_WithCreatedItem()
        {
            // ARRANGE
            var request = new CreateProductLineItemRequestDto { BarCodeId = "999" };
            var response = new ProductLineItemDto { Id = "PL2", BarCodeId = "999" };

            _serviceMock.Setup(s => s.InsertProductLineItem(request)).ReturnsAsync(response);

            // ACT
            var result = await _controller.Create(request);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ProductLineItemDto>(okResult.Value);
            Assert.Equal("PL2", returnValue.Id);
        }

        [Fact]
        public async Task Update_ShouldReturnOk_WhenFound()
        {
            // ARRANGE
            var id = "PL1";
            var request = new UpdateProductLineItemRequestDto { BarCodeId = "888" };
            var response = new ProductLineItemDto { Id = id, BarCodeId = "888" };

            _serviceMock.Setup(s => s.UpdateProductLineItem(request, id)).ReturnsAsync(response);

            // ACT
            var result = await _controller.Update(id, request);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ProductLineItemDto>(okResult.Value);
            Assert.Equal("888", returnValue.BarCodeId);
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_WhenServiceReturnsNull()
        {
            // ARRANGE
            var id = "99";
            var request = new UpdateProductLineItemRequestDto();

            _serviceMock.Setup(s => s.UpdateProductLineItem(request, id)).ReturnsAsync((ProductLineItemDto?)null);

            // ACT
            var result = await _controller.Update(id, request);

            // ASSERT
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnOk_WhenTrue()
        {
            // ARRANGE
            var id = "PL1";
            _serviceMock.Setup(s => s.DeleteProductLineItem(id)).ReturnsAsync(true);

            // ACT
            var result = await _controller.Delete(id);

            // ASSERT
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenFalse()
        {
            // ARRANGE
            var id = "99";
            _serviceMock.Setup(s => s.DeleteProductLineItem(id)).ReturnsAsync(false);

            // ACT
            var result = await _controller.Delete(id);

            // ASSERT
            Assert.IsType<NotFoundResult>(result);
        }

        // --- SPECIAL TEST: GetLineItemByProduct ---

        [Fact]
        public async Task GetLineItemByProduct_ShouldReturnOk_WhenFound()
        {
            // ARRANGE
            var productId = "P1";
            var dtoList = new List<ProductLineItemDto?>
            {
                new ProductLineItemDto { Id = "PL1", BarCodeId = "123" }
            };

            _serviceMock.Setup(s => s.GetLineItemByProductIdAsync(productId)).ReturnsAsync(dtoList);

            // ACT
            var result = await _controller.GetLineItemByProduct(productId);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<ProductLineItemDto?>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task GetLineItemByProduct_ShouldReturnNotFound_WhenNull()
        {
            // ARRANGE
            var productId = "INVALID";
            // Mocking the specific case where service returns null
            _serviceMock.Setup(s => s.GetLineItemByProductIdAsync(productId))
                        .ReturnsAsync((List<ProductLineItemDto?>?)null);

            // ACT
            var result = await _controller.GetLineItemByProduct(productId);

            // ASSERT
            Assert.IsType<NotFoundResult>(result);
        }
    }
}