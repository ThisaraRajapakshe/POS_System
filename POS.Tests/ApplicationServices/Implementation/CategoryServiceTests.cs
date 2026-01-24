using Xunit;
using Moq;
using AutoMapper;
using POS_System.ApplicationServices.Implementation;
using POS_System.Models.Domain;
using POS_System.Models.Dto;
using POS_System.Repositories;

namespace POS.Tests.ApplicationServices.Implementation
{
    public class CategoryServiceTests
    {
        // 1. Define Mocks for the Dependencies
        private readonly Mock<ICategoryRepository> _repoMock;
        private readonly Mock<IMapper> _mapperMock;

        // 2. Define the Real Service
        private readonly CategoryService _service;

        // 3. Constructor: Runs before EVERY test
        public CategoryServiceTests()
        {
            _repoMock = new Mock<ICategoryRepository>();
            _mapperMock = new Mock<IMapper>();

            // Inject both mocks into the service
            _service = new CategoryService(_mapperMock.Object, _repoMock.Object);
        }

        [Fact]
        public async Task GetCategories_ShouldReturnList_WhenCalled()
        {
            // ARRANGE (Setup Data)
            // 1. Fake data coming FROM the database
            var domainList = new List<Category>
            {
                new Category { Id = Guid.NewGuid().ToString(), Name = "Drinks" },
                new Category { Id = Guid.NewGuid().ToString(), Name = "Snacks" }
            };

            // 2. Fake data to return to the user (DTOs)
            var dtoList = new List<CategoryDto>
            {
                new CategoryDto { Id = domainList[0].Id, Name = "Drinks" },
                new CategoryDto { Id = domainList[1].Id, Name = "Snacks" }
            };

            // 3. Setup Repository Mock: "When GetAsync() is called, return domainList"
            _repoMock.Setup(repo => repo.GetAsync())
                     .ReturnsAsync(domainList);

            // 4. Setup Mapper Mock: "When mapping List<Category> to List<CategoryDto>, return dtoList"
            _mapperMock.Setup(m => m.Map<List<CategoryDto>>(domainList))
                       .Returns(dtoList);

            // ACT
            var result = await _service.GetCategories();

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Drinks", result[0].Name);

            // Verify our mocks were actually used
            _repoMock.Verify(r => r.GetAsync(), Times.Once);
        }

        [Fact]
        public async Task GetCategory_ShouldReturnDto_WhenIdExists()
        {
            // ARRANGE
            var id = Guid.NewGuid().ToString(); // Assuming your ID is a string based on your interface
            var domainModel = new Category { Id = "CAT-1", Name = "Tech" };
            var dtoModel = new CategoryDto { Id = "CAT-1", Name = "Tech" };

            // Setup Repo to find the item
            _repoMock.Setup(repo => repo.GetAsync(id)).ReturnsAsync(domainModel);

            // Setup Mapper to convert it
            _mapperMock.Setup(m => m.Map<CategoryDto>(domainModel)).Returns(dtoModel);

            // ACT
            var result = await _service.GetCategory(id);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("Tech", result.Name);
        }

        [Fact]
        public async Task GetCategory_ShouldReturnNull_WhenIdDoesNotExist()
        {
            // ARRANGE
            var id = "non-existent-id";

            // Setup Repo to return null
            _repoMock.Setup(repo => repo.GetAsync(id)).ReturnsAsync((Category?)null);

            // ACT
            var result = await _service.GetCategory(id);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task InsertCategory_ShouldCallRepo_AndReturnDto()
        {
            // ARRANGE
            var requestDto = new CreateCategoryRequestDto { Name = "New Cat" };
            var domainToCreate = new Category { Name = "New Cat" };
            var createdDomain = new Category { Id = Guid.NewGuid().ToString(), Name = "New Cat" };
            var resultDto = new CategoryDto { Id = createdDomain.Id, Name = "New Cat" };

            // 1. Mapper: DTO -> Domain
            _mapperMock.Setup(m => m.Map<Category>(requestDto)).Returns(domainToCreate);

            // 2. Repo: Create the item (simulating DB assigning an ID)
            _repoMock.Setup(r => r.CreateAsync(domainToCreate)).ReturnsAsync(createdDomain);

            // 3. Mapper: Domain -> DTO (Result)
            _mapperMock.Setup(m => m.Map<CategoryDto>(createdDomain)).Returns(resultDto);

            // ACT
            var result = await _service.InsertCategory(requestDto);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(createdDomain.Id, result.Id);
            _repoMock.Verify(r => r.CreateAsync(domainToCreate), Times.Once);
        }

        [Fact]
        public async Task DeleteCategory_ShouldReturnTrue_WhenSuccessful()
        {
            // ARRANGE
            var id = "some-id";
            _repoMock.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);

            // ACT
            var result = await _service.DeleteCategory(id);

            // ASSERT
            Assert.True(result);
        }
    }
}