using Xunit;
using Moq;
using AutoMapper;
using POS_System.ApplicationServices.Implementation;
using POS_System.Models.Domain;
using POS_System.Models.Dto;
using POS_System.Repositories;

namespace POS.Tests.ApplicationServices.Implementation
{
    public class ProductLineItemServiceTests
    {
        private readonly Mock<IProductLineItemRepository> _repoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ProductLineItemService _service;

        public ProductLineItemServiceTests()
        {
            _repoMock = new Mock<IProductLineItemRepository>();
            _mapperMock = new Mock<IMapper>();
            _service = new ProductLineItemService(_repoMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetProductLineItems_ShouldReturnList_WhenCalled()
        {
            // ARRANGE
            var domainList = new List<ProductLineItem>
            {
                new ProductLineItem { Id = "PL1", BarCodeId = "123" }
            };
            var dtoList = new List<ProductLineItemDto>
            {
                new ProductLineItemDto { Id = "PL1", BarCodeId = "123" }
            };

            // NOTE: Your service calls GetAllWithNavPropsAsync here!
            _repoMock.Setup(r => r.GetAllWithNavPropsAsync()).ReturnsAsync(domainList);
            _mapperMock.Setup(m => m.Map<List<ProductLineItemDto>>(domainList)).Returns(dtoList);

            // ACT
            var result = await _service.GetProductLineItems();

            // ASSERT
            Assert.NotNull(result);
            Assert.Single(result);
            _repoMock.Verify(r => r.GetAllWithNavPropsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProductLineItem_ShouldReturnDto_WhenExists()
        {
            // ARRANGE
            var id = "PL1";
            var domainModel = new ProductLineItem { Id = id, BarCodeId = "123" };
            var dtoModel = new ProductLineItemDto { Id = id, BarCodeId = "123" };

            _repoMock.Setup(r => r.GetAsync(id)).ReturnsAsync(domainModel);
            _mapperMock.Setup(m => m.Map<ProductLineItemDto>(domainModel)).Returns(dtoModel);

            // ACT
            var result = await _service.GetProductLineItem(id);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("123", result.BarCodeId);
        }

        [Fact]
        public async Task GetLineItemByProductIdAsync_ShouldReturnList_WhenFound()
        {
            // ARRANGE
            var prodId = "P1";
            var domainList = new List<ProductLineItem> { new ProductLineItem { Id = "PL1" } };
            var dtoList = new List<ProductLineItemDto?> { new ProductLineItemDto { Id = "PL1" } };

            _repoMock.Setup(r => r.GetLineItemByProduct(prodId)).ReturnsAsync(domainList);
            _mapperMock.Setup(m => m.Map<List<ProductLineItemDto?>>(domainList)).Returns(dtoList);

            // ACT
            var result = await _service.GetLineItemByProductIdAsync(prodId);

            // ASSERT
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetLineItemByProductIdAsync_ShouldReturnEmptyList_WhenNull()
        {
            // ARRANGE
            var prodId = "P1";
            // Repo returns null
            _repoMock.Setup(r => r.GetLineItemByProduct(prodId)).ReturnsAsync((List<ProductLineItem>?)null);

            // ACT
            var result = await _service.GetLineItemByProductIdAsync(prodId);

            // ASSERT
            Assert.NotNull(result);
            Assert.Empty(result); // Service should handle null and return empty list
        }

        [Fact]
        public async Task InsertProductLineItem_ShouldReturnDto()
        {
            // ARRANGE
            var request = new CreateProductLineItemRequestDto { BarCodeId = "999" };
            var domain = new ProductLineItem { BarCodeId = "999" };
            var createdDomain = new ProductLineItem { Id = "NEW", BarCodeId = "999" };
            var responseDto = new ProductLineItemDto { Id = "NEW", BarCodeId = "999" };

            _mapperMock.Setup(m => m.Map<ProductLineItem>(request)).Returns(domain);
            _repoMock.Setup(r => r.CreateAsync(domain)).ReturnsAsync(createdDomain);
            _mapperMock.Setup(m => m.Map<ProductLineItemDto>(createdDomain)).Returns(responseDto);

            // ACT
            var result = await _service.InsertProductLineItem(request);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("NEW", result.Id);
        }

        [Fact]
        public async Task UpdateProductLineItem_ShouldReturnDto_WhenFound()
        {
            // ARRANGE
            var id = "PL1";
            var request = new UpdateProductLineItemRequestDto { BarCodeId = "888" };
            var domain = new ProductLineItem { BarCodeId = "888" };
            var updatedDomain = new ProductLineItem { Id = id, BarCodeId = "888" };
            var responseDto = new ProductLineItemDto { Id = id, BarCodeId = "888" };

            _mapperMock.Setup(m => m.Map<ProductLineItem>(request)).Returns(domain);
            _repoMock.Setup(r => r.UpdateAsync(domain, id)).ReturnsAsync(updatedDomain);
            _mapperMock.Setup(m => m.Map<ProductLineItemDto>(updatedDomain)).Returns(responseDto);

            // ACT
            var result = await _service.UpdateProductLineItem(request, id);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("888", result.BarCodeId);
        }

        [Fact]
        public async Task UpdateProductLineItem_ShouldReturnNull_WhenNotFound()
        {
            // ARRANGE
            var id = "PL1";
            var request = new UpdateProductLineItemRequestDto();
            var domain = new ProductLineItem();

            _mapperMock.Setup(m => m.Map<ProductLineItem>(request)).Returns(domain);
            // Repo returns null
            _repoMock.Setup(r => r.UpdateAsync(domain, id)).ReturnsAsync((ProductLineItem?)null);

            // ACT
            var result = await _service.UpdateProductLineItem(request, id);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteProductLineItem_ShouldReturnTrue()
        {
            // ARRANGE
            var id = "PL1";
            _repoMock.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);

            // ACT
            var result = await _service.DeleteProductLineItem(id);

            // ASSERT
            Assert.True(result);
        }
    }
}