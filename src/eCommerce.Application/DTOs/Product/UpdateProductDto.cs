namespace eCommerce.Application.DTOs.Product;

public record UpdateProductDto(
    Guid? CategoryId,
    string Name,
    string? Description,
    int QuantityInStock,
    decimal UnitPrice
);
