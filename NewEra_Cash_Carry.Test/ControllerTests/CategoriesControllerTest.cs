using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewEra_Cash_Carry.Controllers.V1Controllers;
using NewEra_Cash_Carry.DTOs.CategoryDTOs;
using NewEra_Cash_Carry.Interfaces;
using NewEra_Cash_Carry.Models;
using Xunit;

namespace NewEra_Cash_Carry.Test.ControllerTests
{
    public class CategoriesControllerTests
    {
        private readonly Mock<ICategoryRepository> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CategoriesController _controller;

        public CategoriesControllerTests()
        {
            // Mock obyektlar
            _mockRepo = new Mock<ICategoryRepository>();
            _mockMapper = new Mock<IMapper>();

            // Controller yaratish
            _controller = new CategoriesController(null, _mockMapper.Object, _mockRepo.Object);
        }

        // 1. GET: api/Categories
        [Fact]
        public async Task GetCategories_ReturnsOkResultWithList()
        {
            // Arrange
            var mockCategories = new List<CategoryResultDto>
            {
                new CategoryResultDto { Id = 1, Name = "Category 1", Description = "Description 1" },
                new CategoryResultDto { Id = 2, Name = "Category 2", Description = "Description 2" }
            };
            _mockRepo.Setup(repo => repo.GetCategories()).ReturnsAsync(mockCategories);

            // Act
            var result = await _controller.GetCategories();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var categories = Assert.IsType<List<CategoryResultDto>>(okResult.Value);
            Assert.Equal(2, categories.Count);
        }

        // 2. GET: api/Categories/{id} - Valid Id
        [Fact]
        public async Task GetCategory_ValidId_ReturnsOkResult()
        {
            // Arrange
            var mockCategory = new Category { Id = 1, Name = "Category 1", Description = "Description 1" };
            _mockRepo.Setup(repo => repo.GetCategory(It.IsAny<int>())).ReturnsAsync(mockCategory);
            _mockMapper.Setup(mapper => mapper.Map<CategoryResultDto>(mockCategory))
                       .Returns(new CategoryResultDto { Id = 1, Name = "Category 1", Description = "Description 1" });

            // Act
            var result = await _controller.GetCategory(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var category = Assert.IsType<CategoryResultDto>(okResult.Value);
            Assert.Equal(1, category.Id);
        }

        // 3. GET: api/Categories/{id} - Invalid Id
        [Fact]
        public async Task GetCategory_InvalidId_ReturnsNotFound()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetCategory(It.IsAny<int>())).ReturnsAsync((Category)null);

            // Act
            var result = await _controller.GetCategory(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        // 4. POST: api/Categories - Valid Model
        [Fact]
        public async Task AddCategoryAsync_ValidModel_ReturnsCreatedAtAction()
        {
            // Arrange
            var createDto = new CategoryCreateDto { Name = "New Category", Description = "New Description" };
            var mockCategory = new Category { Id = 1, Name = "New Category", Description = "New Description" };

            _mockMapper.Setup(mapper => mapper.Map<Category>(createDto)).Returns(mockCategory);
            _mockRepo.Setup(repo => repo.AddCategoryAsync(mockCategory)).ReturnsAsync(mockCategory);
            _mockMapper.Setup(mapper => mapper.Map<CategoryResultDto>(mockCategory))
                       .Returns(new CategoryResultDto { Id = 1, Name = "New Category", Description = "New Description" });

            // Act
            var result = await _controller.AddCategoryAsync(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var category = Assert.IsType<CategoryResultDto>(createdResult.Value);
            Assert.Equal("New Category", category.Name);
        }

        // 5. POST: api/Categories - Invalid Model
        [Fact]
        public async Task AddCategoryAsync_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var createDto = new CategoryCreateDto();
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _controller.AddCategoryAsync(createDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        // 6. PUT: api/Categories/{id} - Valid Id
        [Fact]
        public async Task UpdateCategory_ValidId_ReturnsNoContent()
        {
            // Arrange
            var updateDto = new CategoryUpdateDto { Name = "Updated Name", Description = "Updated Description" };
            var mockCategory = new Category { Id = 1, Name = "Old Name", Description = "Old Description" };

            _mockRepo.Setup(repo => repo.GetCategory(It.IsAny<int>())).ReturnsAsync(mockCategory);
            _mockRepo.Setup(repo => repo.UpdateCategoryAsync(mockCategory)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateCategory(1, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        // 7. DELETE: api/Categories/{id} - Valid Id
        [Fact]
        public async Task DeleteCategory_ValidId_ReturnsNoContent()
        {
            // Arrange
            var mockCategory = new Category { Id = 1, Name = "Category 1", Description = "Description 1" };

            _mockRepo.Setup(repo => repo.GetCategory(It.IsAny<int>())).ReturnsAsync(mockCategory);
            _mockRepo.Setup(repo => repo.DeleteCategoryAsync(mockCategory)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteCategory(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        // 8. DELETE: api/Categories/{id} - Invalid Id
        [Fact]
        public async Task DeleteCategory_InvalidId_ReturnsNotFound()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetCategory(It.IsAny<int>())).ReturnsAsync((Category)null);

            // Act
            var result = await _controller.DeleteCategory(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
