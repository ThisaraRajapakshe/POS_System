using Xunit;
using Moq;
using AutoMapper;
using POS_System.ApplicationServices.Implementation;
using POS_System.Models.Domain;
using POS_System.Models.Dto;
using POS_System.Repositories;

namespace POS.Tests.ApplicationServices.Implementation
{
    public class ProductServiceTests
    {
        // 1. Define Mocks
        private readonly Mock<IProductRepository> _repoMock;
        private readonly Mock<IMapper> _mapperMock;

        // 2. Define Service
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _repoMock = new Mock<IProductRepository>();
            _mapperMock = new Mock<IMapper>();

            // Inject mocks
            _service = new ProductService(_repoMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetProducts_ShouldReturnList_WhenCalled()
        {
            // ARRANGE
            var domainList = new List<Product>
            {
                new Product { Id = "P1", Name = "Coke" },
                new Product { Id = "P2", Name = "Pepsi" }
            };
            var dtoList = new List<ProductDto>
            {
                new ProductDto { Id = "P1", Name = "Coke" },
                new ProductDto { Id = "P2", Name = "Pepsi" }
            };

            // Note: Your service calls GetAllWithCategoryAsync, NOT GetAsync
            _repoMock.Setup(r => r.GetAllWithCategoryAsync()).ReturnsAsync(domainList);
            _mapperMock.Setup(m => m.Map<List<ProductDto>>(domainList)).Returns(dtoList);

            // ACT
            var result = await _service.GetProducts();

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _repoMock.Verify(r => r.GetAllWithCategoryAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProduct_ShouldReturnDto_WhenExists()
        {
            // ARRANGE
            var id = "P1";
            var domainModel = new Product { Id = id, Name = "Coke" };
            var dtoModel = new ProductDto { Id = id, Name = "Coke" };

            _repoMock.Setup(r => r.GetAsync(id)).ReturnsAsync(domainModel);
            _mapperMock.Setup(m => m.Map<ProductDto>(domainModel)).Returns(dtoModel);

            // ACT
            var result = await _service.GetProduct(id);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("Coke", result.Name);
        }

        [Fact]
        public async Task GetProduct_ShouldReturnNull_WhenNotExists()
        {
            // ARRANGE
            var id = "wrong-id";
            _repoMock.Setup(r => r.GetAsync(id)).ReturnsAsync((Product?)null);

            // ACT
            var result = await _service.GetProduct(id);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task InsertProduct_ShouldReturnDto_WhenSuccessful()
        {
            // ARRANGE
            var requestDto = new CreateProductRequestDto { Name = "Sprite" };
            var domainModel = new Product { Name = "Sprite" };
            var createdDomain = new Product { Id = "P-NEW", Name = "Sprite" };
            var resultDto = new ProductDto { Id = "P-NEW", Name = "Sprite" };

            // Flow: Request -> Domain -> Repo(Create) -> Domain(with ID) -> DTO
            _mapperMock.Setup(m => m.Map<Product>(requestDto)).Returns(domainModel);
            _repoMock.Setup(r => r.CreateAsync(domainModel)).ReturnsAsync(createdDomain);
            _mapperMock.Setup(m => m.Map<ProductDto>(createdDomain)).Returns(resultDto);

            // ACT
            var result = await _service.InsertProduct(requestDto);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("P-NEW", result.Id);
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnDto_WhenFound()
        {
            // ARRANGE
            var id = "P1";
            var requestDto = new UpdateProductRequestDto { Name = "Coke Zero" };
            var domainModel = new Product { Name = "Coke Zero" }; // Mapped from request
            var updatedDomain = new Product { Id = id, Name = "Coke Zero" }; // Returned from DB
            var resultDto = new ProductDto { Id = id, Name = "Coke Zero" };

            _mapperMock.Setup(m => m.Map<Product>(requestDto)).Returns(domainModel);
            _repoMock.Setup(r => r.UpdateAsync(domainModel, id)).ReturnsAsync(updatedDomain);
            _mapperMock.Setup(m => m.Map<ProductDto>(updatedDomain)).Returns(resultDto);

            // ACT
            var result = await _service.UpdateProduct(requestDto, id);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("Coke Zero", result.Name);
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnNull_WhenNotFound()
        {
            // ARRANGE
            var id = "P1";
            var requestDto = new UpdateProductRequestDto { Name = "New Name" };
            var domainModel = new Product { Name = "New Name" };

            _mapperMock.Setup(m => m.Map<Product>(requestDto)).Returns(domainModel);
            // Repo returns NULL (update failed/not found)
            _repoMock.Setup(r => r.UpdateAsync(domainModel, id)).ReturnsAsync((Product?)null);

            // ACT
            var result = await _service.UpdateProduct(requestDto, id);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteProduct_ShouldReturnTrue()
        {
            // ARRANGE
            var id = "P1";
            _repoMock.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);

            // ACT
            var result = await _service.DeleteProduct(id);

            // ASSERT
            Assert.True(result);
        }

        [Fact]
        public async Task GetProductsByCategory_ShouldReturnList_WhenFound()
        {
            // ARRANGE
            var catId = "C1";
            var domainList = new List<Product> { new Product { Id = "P1" } };
            var dtoList = new List<ProductDto?> { new ProductDto { Id = "P1" } };

            _repoMock.Setup(r => r.GetProductsByCategoryAsync(catId)).ReturnsAsync(domainList);
            _mapperMock.Setup(m => m.Map<List<ProductDto?>>(domainList)).Returns(dtoList);

            // ACT
            var result = await _service.GetProductsByCategory(catId);

            // ASSERT
            Assert.NotEmpty(result);
            Assert.Equal(1, result.Count);
        }

        [Fact]
        public async Task GetProductsByCategory_ShouldReturnEmptyList_WhenNullReturned()
        {
            // ARRANGE
            var catId = "C1";
            // Repo returns null
            _repoMock.Setup(r => r.GetProductsByCategoryAsync(catId)).ReturnsAsync((List<Product>?)null);

            // ACT
            var result = await _service.GetProductsByCategory(catId);

            // ASSERT
            Assert.NotNull(result);
            Assert.Empty(result); // Your service manually returns 'new List<ProductDto?>()'
        }
    }
}