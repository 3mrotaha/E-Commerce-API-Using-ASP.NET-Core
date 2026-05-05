namespace eCommerce.Application.DTOs.ProductCategory;

public record ProductCategoryResponseDto(
    Guid Id,
    string Name,
    string? Description
);
