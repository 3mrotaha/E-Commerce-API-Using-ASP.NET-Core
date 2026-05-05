namespace eCommerce.Application.DTOs.ProductReview;

public record ProductReviewResponseDto(
    Guid Id,
    Guid ProductId,
    Guid UserId,
    int Stars,
    string? Comment,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
