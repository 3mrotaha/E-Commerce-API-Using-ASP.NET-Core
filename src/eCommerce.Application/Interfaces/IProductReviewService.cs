using eCommerce.Application.DTOs.ProductReview;
using eCommerce.Application.Common;

namespace eCommerce.Application.Interfaces;

public interface IProductReviewService
{
    Task<Result<IEnumerable<ProductReviewResponseDto>>> GetReviewsByProductIdAsync(Guid productId);
    Task<Result<ProductReviewResponseDto>> GetReviewByIdAsync(Guid reviewId);
    Task<Result<ProductReviewResponseDto>> CreateReviewAsync(Guid userId, CreateProductReviewDto reviewCreateDto);
    Task<Result<ProductReviewResponseDto>> UpdateReviewAsync(Guid reviewId, Guid userId, UpdateProductReviewDto reviewUpdateDto);
    Task<Result<bool>> DeleteReviewAsync(Guid reviewId, Guid userId);
}
