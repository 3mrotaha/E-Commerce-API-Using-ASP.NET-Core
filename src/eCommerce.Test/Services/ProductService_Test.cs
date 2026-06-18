using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using AutoMapper;
using eCommerce.Application.Common;
using eCommerce.Application.DTOs.Product;
using eCommerce.Application.Services;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace eCommerce.Test.Services;

public class ProductService_Test
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<IProductCategoryRepository> _categories = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IDistributedCache> _cache = new();
    private readonly ProductService _sut;

    public ProductService_Test()
    {
        _uow.SetupGet(u => u.Products).Returns(_products.Object);
        _uow.SetupGet(u => u.Categories).Returns(_categories.Object);
        // Default cache = miss; writes/removes are no-ops. Individual tests override GetAsync for hits.
        _cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);
        _cache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
              .Returns(Task.CompletedTask);
        _cache.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _sut = new ProductService(_uow.Object, _mapper.Object, NullLogger<ProductService>.Instance, _cache.Object);
    }

    private static ProductResponseDto SampleDto(Guid? id = null) =>
        new(id ?? Guid.NewGuid(), null, "Cat", "Name", "Desc", 5, 9.99m, default, default);

    // Encodes a value as the cached JSON bytes the service would read back on a hit.
    private void CacheReturns<T>(T value) =>
        _cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value)));

    [Fact]
    public async Task CreateProductAsync_WhenCategoryNotFound_ShouldReturnNotFound()
    {
        var dto = new CreateProductDto(Guid.NewGuid(), "Name", null, 5, 9.99m);
        _categories.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ProductCategory?)null);

        var result = await _sut.CreateProductAsync(dto);

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CreateProductAsync_WhenRepositoryReturnsNull_ShouldReturnBadRequest()
    {
        _mapper.Setup(m => m.Map<Product>(It.IsAny<CreateProductDto>())).Returns(new Product());
        _products.Setup(r => r.AddAsync(It.IsAny<Product>())).ReturnsAsync((Product?)null);

        var result = await _sut.CreateProductAsync(new CreateProductDto(null, "Name", null, 5, 9.99m));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task CreateProductAsync_WhenValid_ShouldAssignIdAndReturnSuccess()
    {
        var product = new Product { Id = Guid.Empty }; // empty → service should assign a new Guid
        _mapper.Setup(m => m.Map<Product>(It.IsAny<CreateProductDto>())).Returns(product);
        _products.Setup(r => r.AddAsync(product)).ReturnsAsync(product);
        _products.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(Array.Empty<Product>());
        _mapper.Setup(m => m.Map<ProductResponseDto>(It.IsAny<Product>())).Returns(SampleDto());

        var result = await _sut.CreateProductAsync(new CreateProductDto(null, "Name", null, 5, 9.99m));

        result.IsSuccess.Should().BeTrue();
        product.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task DeleteProductAsync_WhenNotFound_ShouldReturnNotFound()
    {
        _products.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var result = await _sut.DeleteProductAsync(Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task DeleteProductAsync_WhenFound_ShouldDeleteInvalidateCacheAndReturnSuccess()
    {
        var product = new Product { Id = Guid.NewGuid() };
        _products.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);

        var result = await _sut.DeleteProductAsync(product.Id);

        result.IsSuccess.Should().BeTrue();
        _products.Verify(r => r.DeleteAsync(product), Times.Once);
        _cache.Verify(c => c.RemoveAsync($"product:{product.Id}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllProductsAsync_WhenCacheMiss_ShouldLoadFromRepositoryAndCache()
    {
        _products.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { new Product() });
        var mapped = new List<ProductResponseDto> { SampleDto() };
        _mapper.Setup(m => m.Map<List<ProductResponseDto>>(It.IsAny<object>())).Returns(mapped);

        var result = await _sut.GetAllProductsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        _cache.Verify(c => c.SetAsync("products:all", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllProductsAsync_WhenCacheHit_ShouldNotQueryRepository()
    {
        CacheReturns(new List<ProductResponseDto> { SampleDto() });

        var result = await _sut.GetAllProductsAsync();

        result.IsSuccess.Should().BeTrue();
        _products.Verify(r => r.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenCacheMissAndNotFound_ShouldReturnNotFound()
    {
        _products.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(Array.Empty<Product>());

        var result = await _sut.GetProductByIdAsync(Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenCacheMissAndFound_ShouldReturnSuccess()
    {
        _products.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(new[] { new Product() });
        _mapper.Setup(m => m.Map<ProductResponseDto>(It.IsAny<Product>())).Returns(SampleDto());

        var result = await _sut.GetProductByIdAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenCacheHit_ShouldReturnSuccessWithoutRepository()
    {
        CacheReturns(SampleDto());

        var result = await _sut.GetProductByIdAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        _products.Verify(r => r.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()), Times.Never);
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenCachedValueDeserializesToNull_ShouldReturnFailure()
    {
        // A literal "null" JSON payload deserializes to a null DTO → the failure branch.
        _cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(Encoding.UTF8.GetBytes("null"));

        var result = await _sut.GetProductByIdAsync(Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public async Task UpdateProductAsync_WhenProductNotFound_ShouldReturnNotFound()
    {
        _products.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var result = await _sut.UpdateProductAsync(Guid.NewGuid(), new UpdateProductDto(null, "N", null, 1, 1m));

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateProductAsync_WhenCategoryNotFound_ShouldReturnNotFound()
    {
        var id = Guid.NewGuid();
        _products.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new Product { Id = id });
        _categories.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ProductCategory?)null);

        var result = await _sut.UpdateProductAsync(id, new UpdateProductDto(Guid.NewGuid(), "N", null, 1, 1m));

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateProductAsync_WhenRepositoryReturnsNull_ShouldReturnBadRequest()
    {
        var id = Guid.NewGuid();
        _products.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new Product { Id = id });
        _products.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync((Product?)null);

        var result = await _sut.UpdateProductAsync(id, new UpdateProductDto(null, "N", null, 1, 1m));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task UpdateProductAsync_WhenValid_ShouldUpdateInvalidateCacheAndReturnSuccess()
    {
        var id = Guid.NewGuid();
        var product = new Product { Id = id, Name = "Old" };
        _products.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(product);
        _products.Setup(r => r.UpdateAsync(product)).ReturnsAsync(product);
        _products.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(Array.Empty<Product>());
        _mapper.Setup(m => m.Map<ProductResponseDto>(It.IsAny<Product>())).Returns(SampleDto(id));

        var result = await _sut.UpdateProductAsync(id, new UpdateProductDto(null, "New", "desc", 9, 12m));

        result.IsSuccess.Should().BeTrue();
        product.Name.Should().Be("New"); // DTO fields copied onto the entity
        _cache.Verify(c => c.RemoveAsync($"product:{id}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllProductsFiltered_WhenNoSearchNoCategory_ShouldQueryPagedRepository()
    {
        _products.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<int>(), It.IsAny<int>()))
                 .ReturnsAsync(new[] { new Product() });
        _mapper.Setup(m => m.Map<List<ProductResponseDto>>(It.IsAny<object>())).Returns(new List<ProductResponseDto> { SampleDto() });

        var result = await _sut.GetAllProductsAsync(null, null, null, null, 1, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllProductsFiltered_WhenCategoryNameUnknown_ShouldReturnNotFound()
    {
        _categories.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ProductCategory, bool>>>())).ReturnsAsync(Array.Empty<ProductCategory>());

        var result = await _sut.GetAllProductsAsync(null, "Ghost", null, null, 1, 10);

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetAllProductsFiltered_WhenSearchAndCategoryProvided_ShouldQueryPagedRepository()
    {
        _categories.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ProductCategory, bool>>>()))
                   .ReturnsAsync(new[] { new ProductCategory { Id = Guid.NewGuid(), Name = "Electronics" } });
        _products.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<int>(), It.IsAny<int>()))
                 .ReturnsAsync(new[] { new Product() });
        _mapper.Setup(m => m.Map<List<ProductResponseDto>>(It.IsAny<object>())).Returns(new List<ProductResponseDto> { SampleDto() });

        var result = await _sut.GetAllProductsAsync("phone", "Electronics", -5m, 1000m, 1, 10);

        result.IsSuccess.Should().BeTrue(); // negative minPrice is clamped to 0 internally
    }

    [Fact]
    public async Task GetAllProductsFiltered_WhenCacheHit_ShouldNotQueryRepository()
    {
        CacheReturns(new List<ProductResponseDto> { SampleDto() });

        var result = await _sut.GetAllProductsAsync(null, null, null, null, 1, 10);

        result.IsSuccess.Should().BeTrue();
        _products.Verify(r => r.FindAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }
}
