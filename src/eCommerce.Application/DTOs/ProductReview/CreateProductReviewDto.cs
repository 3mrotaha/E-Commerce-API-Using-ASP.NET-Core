namespace eCommerce.Application.DTOs.ProductReview;

public record CreateProductReviewDto(
    Guid ProductId,
    int Stars,
    string? Comment
);
