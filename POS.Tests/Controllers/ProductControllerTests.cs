using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using POS_System.Controllers;
using POS_System.ApplicationServices;
using POS_System.Models.Dto;

namespace POS.Tests.Controllers
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _serviceMock;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _serviceMock = new Mock<IProductService>();
            _controller = new ProductController(_serviceMock.Object);
        }

        [Fact]
        public async Task Get_ShouldReturnOk_WithList()
        {
            // ARRANGE
            var dtoList = new List<ProductDto>
            {
                new ProductDto { Id = "P1", Name = "Coke" }
            };

            _serviceMock.Setup(s => s.GetProducts()).ReturnsAsync(dtoList);

            // ACT
            var result = await _controller.Get();

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<ProductDto>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenFound()
        {
            // ARRANGE
            var id = "P1";
            var dto = new ProductDto { Id = id, Name = "Coke" };

            _serviceMock.Setup(s => s.GetProduct(id)).ReturnsAsync(dto);

            // ACT
            var result = await _controller.Get(id);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ProductDto>(okResult.Value);
            Assert.Equal("Coke", returnValue.Name);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenNull()
        {
            // ARRANGE
            var id = "99";
            _serviceMock.Setup(s => s.GetProduct(id)).ReturnsAsync((ProductDto?)null);

            // ACT
            var result = await _controller.Get(id);

            // ASSERT
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_ShouldReturnOk_WithCreatedItem()
        {
            // ARRANGE
            var request = new CreateProductRequestDto { Name = "Sprite" };
            var response = new ProductDto { Id = "P2", Name = "Sprite" };

            _serviceMock.Setup(s => s.InsertProduct(request)).ReturnsAsync(response);

            // ACT
            var result = await _controller.Create(request);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ProductDto>(okResult.Value);
            Assert.Equal("P2", returnValue.Id);
        }

        [Fact]
        public async Task Update_ShouldReturnOk_WhenFound()
        {
            // ARRANGE
            var id = "P1";
            var request = new UpdateProductRequestDto { Name = "Diet Coke" };
            var response = new ProductDto { Id = id, Name = "Diet Coke" };

            _serviceMock.Setup(s => s.UpdateProduct(request, id)).ReturnsAsync(response);

            // ACT
            var result = await _controller.Update(id, request);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ProductDto>(okResult.Value);
            Assert.Equal("Diet Coke", returnValue.Name);
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_WhenServiceReturnsNull()
        {
            // ARRANGE
            var id = "99";
            var request = new UpdateProductRequestDto();

            _serviceMock.Setup(s => s.UpdateProduct(request, id)).ReturnsAsync((ProductDto?)null);

            // ACT
            var result = await _controller.Update(id, request);

            // ASSERT
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnOk_WhenTrue()
        {
            // ARRANGE
            var id = "P1";
            _serviceMock.Setup(s => s.DeleteProduct(id)).ReturnsAsync(true);

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
            _serviceMock.Setup(s => s.DeleteProduct(id)).ReturnsAsync(false);

            // ACT
            var result = await _controller.Delete(id);

            // ASSERT
            Assert.IsType<NotFoundResult>(result);
        }

        // --- NEW TEST: GetProductsByCategory ---

        [Fact]
        public async Task GetProductsByCategory_ShouldReturnOk_WhenFound()
        {
            // ARRANGE
            var catId = "C1";
            var dtoList = new List<ProductDto?>
            {
                new ProductDto { Id = "P1", Name = "Soda" }
            };

            _serviceMock.Setup(s => s.GetProductsByCategory(catId)).ReturnsAsync(dtoList);

            // ACT
            var result = await _controller.GetProductsbyCategory(catId);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<ProductDto?>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task GetProductsByCategory_ShouldReturnNotFound_WhenNull()
        {
            // ARRANGE
            var catId = "INVALID";
            // Even though your Service returns empty list, the Controller explicitly checks for NULL.
            // So we test that null check here to ensure the Controller logic is covered.
            _serviceMock.Setup(s => s.GetProductsByCategory(catId)).ReturnsAsync((List<ProductDto?>?)null);

            // ACT
            var result = await _controller.GetProductsbyCategory(catId);

            // ASSERT
            Assert.IsType<NotFoundResult>(result);
        }
    }
}