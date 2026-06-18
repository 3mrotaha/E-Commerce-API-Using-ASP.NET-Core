using System.Linq.Expressions;
using AutoMapper;
using eCommerce.Application.Common;
using eCommerce.Application.DTOs.ProductReview;
using eCommerce.Application.Exceptions;
using eCommerce.Application.Services;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace eCommerce.Test.Services;

public class ProductReviewService_Test
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IProductReviewRepository> _reviews = new();
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly ProductReviewService _sut;

    public ProductReviewService_Test()
    {
        _uow.SetupGet(u => u.ProductReviews).Returns(_reviews.Object);
        _uow.SetupGet(u => u.Products).Returns(_products.Object);
        _sut = new ProductReviewService(_uow.Object, _mapper.Object, NullLogger<ProductReviewService>.Instance);
    }

    private void SetupExistingReview(params ProductReview[] matches) =>
        _reviews.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ProductReview, bool>>>())).ReturnsAsync(matches);

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task CreateReviewAsync_WhenStarsOutOfRange_ShouldReturnBadRequest(int stars)
    {
        var result = await _sut.CreateReviewAsync(Guid.NewGuid(), new CreateProductReviewDto(Guid.NewGuid(), stars, null));

        result.Error!.Type.Should().Be(ErrorType.Validation);
        _products.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CreateReviewAsync_WhenProductNotFound_ShouldReturnNotFound()
    {
        _products.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var result = await _sut.CreateReviewAsync(Guid.NewGuid(), new CreateProductReviewDto(Guid.NewGuid(), 4, null));

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task CreateReviewAsync_WhenUserAlreadyReviewed_ShouldReturnConflict()
    {
        _products.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Product { Id = Guid.NewGuid() });
        SetupExistingReview(new ProductReview());

        var result = await _sut.CreateReviewAsync(Guid.NewGuid(), new CreateProductReviewDto(Guid.NewGuid(), 4, null));

        result.Error!.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task CreateReviewAsync_WhenRepositoryReturnsNull_ShouldReturnBadRequest()
    {
        _products.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Product());
        SetupExistingReview();
        _mapper.Setup(m => m.Map<ProductReview>(It.IsAny<CreateProductReviewDto>())).Returns(new ProductReview());
        _reviews.Setup(r => r.AddAsync(It.IsAny<ProductReview>())).ReturnsAsync((ProductReview?)null);

        var result = await _sut.CreateReviewAsync(Guid.NewGuid(), new CreateProductReviewDto(Guid.NewGuid(), 4, null));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task CreateReviewAsync_WhenValid_ShouldReturnSuccessAndStampUser()
    {
        var userId = Guid.NewGuid();
        var review = new ProductReview();
        _products.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Product());
        SetupExistingReview();
        _mapper.Setup(m => m.Map<ProductReview>(It.IsAny<CreateProductReviewDto>())).Returns(review);
        _reviews.Setup(r => r.AddAsync(review)).ReturnsAsync(review);
        _mapper.Setup(m => m.Map<ProductReviewResponseDto>(review))
               .Returns(new ProductReviewResponseDto(review.Id, review.ProductId, userId, 4, null, default, default));

        var result = await _sut.CreateReviewAsync(userId, new CreateProductReviewDto(Guid.NewGuid(), 4, null));

        result.IsSuccess.Should().BeTrue();
        review.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task DeleteReviewAsync_WhenNotFound_ShouldReturnNotFound()
    {
        _reviews.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ProductReview?)null);

        var result = await _sut.DeleteReviewAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task DeleteReviewAsync_WhenNotOwner_ShouldThrowUnauthorized()
    {
        var review = new ProductReview { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _reviews.Setup(r => r.GetByIdAsync(review.Id)).ReturnsAsync(review);

        var act = () => _sut.DeleteReviewAsync(review.Id, Guid.NewGuid());

        await act.Should().ThrowAsync<UnautherizedException>();
    }

    [Fact]
    public async Task DeleteReviewAsync_WhenOwner_ShouldDeleteAndReturnSuccess()
    {
        var userId = Guid.NewGuid();
        var review = new ProductReview { Id = Guid.NewGuid(), UserId = userId };
        _reviews.Setup(r => r.GetByIdAsync(review.Id)).ReturnsAsync(review);

        var result = await _sut.DeleteReviewAsync(review.Id, userId);

        result.IsSuccess.Should().BeTrue();
        _reviews.Verify(r => r.DeleteAsync(review), Times.Once);
    }

    [Fact]
    public async Task GetReviewByIdAsync_WhenNotFound_ShouldReturnNotFound()
    {
        _reviews.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ProductReview?)null);

        var result = await _sut.GetReviewByIdAsync(Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetReviewByIdAsync_WhenFound_ShouldReturnSuccess()
    {
        var review = new ProductReview { Id = Guid.NewGuid() };
        _reviews.Setup(r => r.GetByIdAsync(review.Id)).ReturnsAsync(review);
        _mapper.Setup(m => m.Map<ProductReviewResponseDto>(review))
               .Returns(new ProductReviewResponseDto(review.Id, default, default, 5, null, default, default));

        var result = await _sut.GetReviewByIdAsync(review.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetReviewsByProductIdAsync_WhenProductNotFound_ShouldReturnNotFound()
    {
        _products.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var result = await _sut.GetReviewsByProductIdAsync(Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetReviewsByProductIdAsync_WhenProductExists_ShouldReturnMappedReviews()
    {
        _products.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Product());
        SetupExistingReview(new ProductReview());
        var mapped = new[] { new ProductReviewResponseDto(Guid.NewGuid(), default, default, 5, null, default, default) };
        _mapper.Setup(m => m.Map<IEnumerable<ProductReviewResponseDto>>(It.IsAny<IEnumerable<ProductReview>>()))
               .Returns(mapped);

        var result = await _sut.GetReviewsByProductIdAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(mapped);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task UpdateReviewAsync_WhenStarsOutOfRange_ShouldReturnBadRequest(int stars)
    {
        var result = await _sut.UpdateReviewAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdateProductReviewDto(stars, null));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task UpdateReviewAsync_WhenNotFound_ShouldReturnNotFound()
    {
        _reviews.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ProductReview?)null);

        var result = await _sut.UpdateReviewAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdateProductReviewDto(4, null));

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateReviewAsync_WhenNotOwner_ShouldThrowUnauthorized()
    {
        var review = new ProductReview { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _reviews.Setup(r => r.GetByIdAsync(review.Id)).ReturnsAsync(review);

        var act = () => _sut.UpdateReviewAsync(review.Id, Guid.NewGuid(), new UpdateProductReviewDto(4, null));

        await act.Should().ThrowAsync<UnautherizedException>();
    }

    [Fact]
    public async Task UpdateReviewAsync_WhenRepositoryReturnsNull_ShouldReturnBadRequest()
    {
        var userId = Guid.NewGuid();
        var review = new ProductReview { Id = Guid.NewGuid(), UserId = userId };
        _reviews.Setup(r => r.GetByIdAsync(review.Id)).ReturnsAsync(review);
        _reviews.Setup(r => r.UpdateAsync(review)).ReturnsAsync((ProductReview?)null);

        var result = await _sut.UpdateReviewAsync(review.Id, userId, new UpdateProductReviewDto(4, null));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task UpdateReviewAsync_WhenValid_ShouldReturnSuccessWithUpdatedFields()
    {
        var userId = Guid.NewGuid();
        var review = new ProductReview { Id = Guid.NewGuid(), UserId = userId, Stars = 1 };
        _reviews.Setup(r => r.GetByIdAsync(review.Id)).ReturnsAsync(review);
        _reviews.Setup(r => r.UpdateAsync(review)).ReturnsAsync(review);
        _mapper.Setup(m => m.Map<ProductReviewResponseDto>(review))
               .Returns(new ProductReviewResponseDto(review.Id, default, userId, 5, "great", default, default));

        var result = await _sut.UpdateReviewAsync(review.Id, userId, new UpdateProductReviewDto(5, "great"));

        result.IsSuccess.Should().BeTrue();
        review.Stars.Should().Be(5);
        review.Comment.Should().Be("great");
    }
}
