namespace eCommerce.Application.DTOs.ProductCategory;

public record UpdateProductCategoryDto(
    string Name,
    string? Description
);
