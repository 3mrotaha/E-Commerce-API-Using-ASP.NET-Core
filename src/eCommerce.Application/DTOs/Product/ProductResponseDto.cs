namespace eCommerce.Application.DTOs.Product;

public record ProductResponseDto(
    Guid Id,
    Guid? CategoryId,
    string? CategoryName,
    string Name,
    string? Description,
    int QuantityInStock,
    decimal UnitPrice,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
