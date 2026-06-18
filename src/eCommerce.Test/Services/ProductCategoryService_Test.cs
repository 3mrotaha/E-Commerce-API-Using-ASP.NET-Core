using System.Linq.Expressions;
using AutoMapper;
using eCommerce.Application.Common;
using eCommerce.Application.DTOs.ProductCategory;
using eCommerce.Application.Services;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace eCommerce.Test.Services;

public class ProductCategoryService_Test
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IProductCategoryRepository> _categories = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly ProductCategoryService _sut;

    public ProductCategoryService_Test()
    {
        _uow.SetupGet(u => u.Categories).Returns(_categories.Object);
        _sut = new ProductCategoryService(_uow.Object, _mapper.Object, NullLogger<ProductCategoryService>.Instance);
    }

    // Convenience: stub the "find by name" lookup used to detect duplicates.
    private void SetupFindByName(params ProductCategory[] matches) =>
        _categories.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ProductCategory, bool>>>()))
                   .ReturnsAsync(matches);

    [Fact]
    public async Task CreateCategoryAsync_WhenNameAlreadyExists_ShouldReturnConflict()
    {
        SetupFindByName(new ProductCategory { Name = "Books" });

        var result = await _sut.CreateCategoryAsync(new CreateProductCategoryDto("Books", null));

        result.Error!.Type.Should().Be(ErrorType.Conflict);
        _categories.Verify(r => r.AddAsync(It.IsAny<ProductCategory>()), Times.Never);
    }

    [Fact]
    public async Task CreateCategoryAsync_WhenRepositoryReturnsNull_ShouldReturnBadRequest()
    {
        SetupFindByName();
        _mapper.Setup(m => m.Map<ProductCategory>(It.IsAny<CreateProductCategoryDto>())).Returns(new ProductCategory());
        _categories.Setup(r => r.AddAsync(It.IsAny<ProductCategory>())).ReturnsAsync((ProductCategory?)null);

        var result = await _sut.CreateCategoryAsync(new CreateProductCategoryDto("Books", null));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task CreateCategoryAsync_WhenValid_ShouldReturnSuccessWithAssignedId()
    {
        SetupFindByName();
        var entity = new ProductCategory();
        _mapper.Setup(m => m.Map<ProductCategory>(It.IsAny<CreateProductCategoryDto>())).Returns(entity);
        _categories.Setup(r => r.AddAsync(entity)).ReturnsAsync(entity);
        _mapper.Setup(m => m.Map<ProductCategoryResponseDto>(entity))
               .Returns(new ProductCategoryResponseDto(entity.Id, "Books", null));

        var result = await _sut.CreateCategoryAsync(new CreateProductCategoryDto("Books", null));

        result.IsSuccess.Should().BeTrue();
        entity.Id.Should().NotBe(Guid.Empty); // service assigns a new Guid before persisting
    }

    [Fact]
    public async Task DeleteCategoryAsync_WhenNotFound_ShouldReturnNotFound()
    {
        _categories.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ProductCategory?)null);

        var result = await _sut.DeleteCategoryAsync(Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task DeleteCategoryAsync_WhenFound_ShouldDeleteAndReturnSuccess()
    {
        var category = new ProductCategory { Id = Guid.NewGuid() };
        _categories.Setup(r => r.GetByIdAsync(category.Id)).ReturnsAsync(category);

        var result = await _sut.DeleteCategoryAsync(category.Id);

        result.IsSuccess.Should().BeTrue();
        _categories.Verify(r => r.DeleteAsync(category), Times.Once);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_WhenCalled_ShouldReturnMappedCategories()
    {
        _categories.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { new ProductCategory() });
        var mapped = new[] { new ProductCategoryResponseDto(Guid.NewGuid(), "A", null) };
        _mapper.Setup(m => m.Map<IEnumerable<ProductCategoryResponseDto>>(It.IsAny<IEnumerable<ProductCategory>>()))
               .Returns(mapped);

        var result = await _sut.GetAllCategoriesAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(mapped);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WhenNotFound_ShouldReturnNotFound()
    {
        _categories.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ProductCategory?)null);

        var result = await _sut.GetCategoryByIdAsync(Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WhenFound_ShouldReturnSuccess()
    {
        var category = new ProductCategory { Id = Guid.NewGuid() };
        _categories.Setup(r => r.GetByIdAsync(category.Id)).ReturnsAsync(category);
        _mapper.Setup(m => m.Map<ProductCategoryResponseDto>(category))
               .Returns(new ProductCategoryResponseDto(category.Id, "A", null));

        var result = await _sut.GetCategoryByIdAsync(category.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenNotFound_ShouldReturnNotFound()
    {
        _categories.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ProductCategory?)null);

        var result = await _sut.UpdateCategoryAsync(Guid.NewGuid(), new UpdateProductCategoryDto("A", null));

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenAnotherCategoryHasSameName_ShouldReturnConflict()
    {
        var id = Guid.NewGuid();
        _categories.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new ProductCategory { Id = id });
        SetupFindByName(new ProductCategory { Id = Guid.NewGuid(), Name = "A" }); // a different category owns the name

        var result = await _sut.UpdateCategoryAsync(id, new UpdateProductCategoryDto("A", null));

        result.Error!.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenRepositoryReturnsNull_ShouldReturnBadRequest()
    {
        var id = Guid.NewGuid();
        _categories.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new ProductCategory { Id = id });
        SetupFindByName();
        _categories.Setup(r => r.UpdateAsync(It.IsAny<ProductCategory>())).ReturnsAsync((ProductCategory?)null);

        var result = await _sut.UpdateCategoryAsync(id, new UpdateProductCategoryDto("A", null));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenValid_ShouldReturnSuccessWithUpdatedFields()
    {
        var id = Guid.NewGuid();
        var category = new ProductCategory { Id = id, Name = "Old" };
        _categories.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(category);
        SetupFindByName();
        _categories.Setup(r => r.UpdateAsync(category)).ReturnsAsync(category);
        _mapper.Setup(m => m.Map<ProductCategoryResponseDto>(category))
               .Returns(new ProductCategoryResponseDto(id, "New", "desc"));

        var result = await _sut.UpdateCategoryAsync(id, new UpdateProductCategoryDto("New", "desc"));

        result.IsSuccess.Should().BeTrue();
        category.Name.Should().Be("New"); // service copies DTO fields onto the entity before update
    }
}
