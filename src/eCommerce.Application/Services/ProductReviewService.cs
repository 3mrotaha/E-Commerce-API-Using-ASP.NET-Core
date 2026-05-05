using AutoMapper;
using eCommerce.Application.Common;
using eCommerce.Application.DTOs.ProductReview;
using eCommerce.Application.Exceptions;
using eCommerce.Application.Interfaces;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace eCommerce.Application.Services;

public class ProductReviewService : IProductReviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductReviewService> _logger;

    public ProductReviewService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductReviewService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ProductReviewResponseDto>> CreateReviewAsync(Guid userId, CreateProductReviewDto reviewCreateDto)
    {
        _logger.LogDebug("Creating review for user {UserId} and product {ProductId}", userId, reviewCreateDto.ProductId);

        var starsValidation = ValidateStars(reviewCreateDto.Stars);
        if (starsValidation != null)
        {
            _logger.LogError("Cannot create review for user {UserId}; invalid stars {Stars}", userId, reviewCreateDto.Stars);
            return Result<ProductReviewResponseDto>.BadRequest(starsValidation);
        }

        var product = await _unitOfWork.Products.GetByIdAsync(reviewCreateDto.ProductId);
        if (product == null)
        {
            _logger.LogError("Cannot create review; product {ProductId} was not found", reviewCreateDto.ProductId);
            return Result<ProductReviewResponseDto>.NotFound($"Product with id '{reviewCreateDto.ProductId}' was not found.");
        }

        var existingReview = (await _unitOfWork.ProductReviews.FindAsync(r => r.ProductId == reviewCreateDto.ProductId && r.UserId == userId)).FirstOrDefault();
        if (existingReview != null)
        {
            _logger.LogError("Cannot create review; user {UserId} already reviewed product {ProductId}", userId, reviewCreateDto.ProductId);
            return Result<ProductReviewResponseDto>.Conflict("User has already submitted a review for this product.");
        }

        var review = _mapper.Map<ProductReview>(reviewCreateDto);
        review.Id = Guid.NewGuid();
        review.UserId = userId;
        review.CreatedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;

        var createdReview = await _unitOfWork.ProductReviews.AddAsync(review);
        if (createdReview == null)
        {
            _logger.LogError("Failed to create product review for user {UserId} and product {ProductId}", userId, reviewCreateDto.ProductId);
            return Result<ProductReviewResponseDto>.BadRequest("Failed to create product review.");
        }

        _logger.LogInformation("Created review {ReviewId} for user {UserId} and product {ProductId}", createdReview.Id, userId, reviewCreateDto.ProductId);
        return Result<ProductReviewResponseDto>.Success(_mapper.Map<ProductReviewResponseDto>(createdReview));
    }

    public async Task<Result<bool>> DeleteReviewAsync(Guid reviewId, Guid userId)
    {
        _logger.LogDebug("Deleting review {ReviewId} for user {UserId}", reviewId, userId);

        var review = await _unitOfWork.ProductReviews.GetByIdAsync(reviewId);
        if (review == null)
        {
            _logger.LogError("Cannot delete review {ReviewId}; review was not found", reviewId);
            return Result<bool>.NotFound($"Review with id '{reviewId}' was not found.");
        }

        if (review.UserId != userId)
        {
            _logger.LogCritical("User {UserId} attempted to delete review {ReviewId} owned by user {OwnerUserId}", userId, reviewId, review.UserId);
            throw new UnautherizedException("You are not authorized to delete this review.");
        }

        await _unitOfWork.ProductReviews.DeleteAsync(review);
        _logger.LogInformation("Deleted review {ReviewId} for user {UserId}", reviewId, userId);
        return Result<bool>.Success(true);
    }

    public async Task<Result<ProductReviewResponseDto>> GetReviewByIdAsync(Guid reviewId)
    {
        _logger.LogDebug("Getting review {ReviewId}", reviewId);

        var review = await _unitOfWork.ProductReviews.GetByIdAsync(reviewId);
        if (review == null)
        {
            _logger.LogError("Review {ReviewId} was not found", reviewId);
            return Result<ProductReviewResponseDto>.NotFound($"Review with id '{reviewId}' was not found.");
        }

        _logger.LogInformation("Retrieved review {ReviewId}", reviewId);
        return Result<ProductReviewResponseDto>.Success(_mapper.Map<ProductReviewResponseDto>(review));
    }

    public async Task<Result<IEnumerable<ProductReviewResponseDto>>> GetReviewsByProductIdAsync(Guid productId)
    {
        _logger.LogDebug("Getting reviews for product {ProductId}", productId);

        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
        {
            _logger.LogError("Cannot get reviews; product {ProductId} was not found", productId);
            return Result<IEnumerable<ProductReviewResponseDto>>.NotFound($"Product with id '{productId}' was not found.");
        }

        var reviews = await _unitOfWork.ProductReviews.FindAsync(r => r.ProductId == productId);
        _logger.LogInformation("Retrieved reviews for product {ProductId}", productId);
        return Result<IEnumerable<ProductReviewResponseDto>>.Success(_mapper.Map<IEnumerable<ProductReviewResponseDto>>(reviews.OrderByDescending(r => r.CreatedAt)));
    }

    public async Task<Result<ProductReviewResponseDto>> UpdateReviewAsync(Guid reviewId, Guid userId, UpdateProductReviewDto reviewUpdateDto)
    {
        _logger.LogDebug("Updating review {ReviewId} for user {UserId}", reviewId, userId);

        var starsValidation = ValidateStars(reviewUpdateDto.Stars);
        if (starsValidation != null)
        {
            _logger.LogError("Cannot update review {ReviewId}; invalid stars {Stars}", reviewId, reviewUpdateDto.Stars);
            return Result<ProductReviewResponseDto>.BadRequest(starsValidation);
        }

        var review = await _unitOfWork.ProductReviews.GetByIdAsync(reviewId);
        if (review == null)
        {
            _logger.LogError("Cannot update review {ReviewId}; review was not found", reviewId);
            return Result<ProductReviewResponseDto>.NotFound($"Review with id '{reviewId}' was not found.");
        }

        if (review.UserId != userId)
        {
            _logger.LogCritical("User {UserId} attempted to update review {ReviewId} owned by user {OwnerUserId}", userId, reviewId, review.UserId);
            throw new UnautherizedException("You are not authorized to update this review.");
        }

        review.Stars = reviewUpdateDto.Stars;
        review.Comment = reviewUpdateDto.Comment;
        review.UpdatedAt = DateTime.UtcNow;

        var updatedReview = await _unitOfWork.ProductReviews.UpdateAsync(review);
        if (updatedReview == null)
        {
            _logger.LogError("Failed to update review {ReviewId}", reviewId);
            return Result<ProductReviewResponseDto>.BadRequest("Failed to update product review.");
        }

        _logger.LogInformation("Updated review {ReviewId} for user {UserId}", reviewId, userId);
        return Result<ProductReviewResponseDto>.Success(_mapper.Map<ProductReviewResponseDto>(updatedReview));
    }

    private static string? ValidateStars(int stars)
    {
        if (stars < 1 || stars > 5)
        {
            return "Stars must be between 1 and 5.";
        }

        return null;
    }
}
