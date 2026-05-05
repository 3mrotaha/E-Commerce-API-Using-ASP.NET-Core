namespace eCommerce.Application.DTOs.ProductReview;

public record UpdateProductReviewDto(
    int Stars,
    string? Comment
);
