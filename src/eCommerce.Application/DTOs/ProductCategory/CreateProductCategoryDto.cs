namespace eCommerce.Application.DTOs.ProductCategory;

public record CreateProductCategoryDto(
    string Name,
    string? Description
);
