using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using POS_System.Controllers;
using POS_System.ApplicationServices;
using POS_System.Models.Dto;

namespace POS.Tests.Controllers
{
    public class CategoryControllerTests
    {
        private readonly Mock<ICategoryService> _serviceMock;
        private readonly CategoryController _controller;

        public CategoryControllerTests()
        {
            _serviceMock = new Mock<ICategoryService>();
            _controller = new CategoryController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithList()
        {
            // ARRANGE
            var dtoList = new List<CategoryDto>
            {
                new CategoryDto { Id = "1", Name = "Drinks" }
            };

            _serviceMock.Setup(s => s.GetCategories()).ReturnsAsync(dtoList);

            // ACT
            var result = await _controller.GetAll();

            // ASSERT
            // 1. Check if the Return Type is "OkObjectResult" (HTTP 200)
            var okResult = Assert.IsType<OkObjectResult>(result);

            // 2. Check if the Data inside is correct
            var returnValue = Assert.IsType<List<CategoryDto>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task Get_ShouldReturnOk_WhenFound()
        {
            // ARRANGE
            var id = "1";
            var dto = new CategoryDto { Id = id, Name = "Drinks" };

            _serviceMock.Setup(s => s.GetCategory(id)).ReturnsAsync(dto);

            // ACT
            var result = await _controller.Get(id);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<CategoryDto>(okResult.Value);
            Assert.Equal("Drinks", returnValue.Name);
        }

        [Fact]
        public async Task Get_ShouldReturnNotFound_WhenNull()
        {
            // ARRANGE
            var id = "99";
            _serviceMock.Setup(s => s.GetCategory(id)).ReturnsAsync((CategoryDto?)null);

            // ACT
            var result = await _controller.Get(id);

            // ASSERT
            // Check if it returns "NotFoundResult" (HTTP 404)
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_ShouldReturnOk_WithCreatedItem()
        {
            // ARRANGE
            var request = new CreateCategoryRequestDto { Name = "New Cat" };
            var response = new CategoryDto { Id = "100", Name = "New Cat" };

            _serviceMock.Setup(s => s.InsertCategory(request)).ReturnsAsync(response);

            // ACT
            var result = await _controller.Create(request);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<CategoryDto>(okResult.Value);
            Assert.Equal("100", returnValue.Id);
        }

        [Fact]
        public async Task Update_ShouldReturnOk_WhenFound()
        {
            // ARRANGE
            var id = "1";
            var request = new UpdateCategoryRequestDto { Name = "Updated" };
            var response = new CategoryDto { Id = id, Name = "Updated" };

            _serviceMock.Setup(s => s.UpdateCategory(request, id)).ReturnsAsync(response);

            // ACT
            var result = await _controller.Update(id, request);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_WhenServiceReturnsNull()
        {
            // ARRANGE
            var id = "99";
            var request = new UpdateCategoryRequestDto();

            _serviceMock.Setup(s => s.UpdateCategory(request, id)).ReturnsAsync((CategoryDto?)null);

            // ACT
            var result = await _controller.Update(id, request);

            // ASSERT
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnOk_WhenTrue()
        {
            // ARRANGE
            var id = "1";
            _serviceMock.Setup(s => s.DeleteCategory(id)).ReturnsAsync(true);

            // ACT
            var result = await _controller.Delete(id);

            // ASSERT
            Assert.IsType<OkResult>(result); // OkResult (void), not OkObjectResult (data)
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenFalse()
        {
            // ARRANGE
            var id = "99";
            _serviceMock.Setup(s => s.DeleteCategory(id)).ReturnsAsync(false);

            // ACT
            var result = await _controller.Delete(id);

            // ASSERT
            Assert.IsType<NotFoundResult>(result);
        }
    }
}