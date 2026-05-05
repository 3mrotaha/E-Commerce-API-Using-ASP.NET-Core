namespace eCommerce.Application.DTOs.Product;

public record CreateProductDto(
    Guid? CategoryId,
    string Name,
    string? Description,
    int QuantityInStock,
    decimal UnitPrice
);
