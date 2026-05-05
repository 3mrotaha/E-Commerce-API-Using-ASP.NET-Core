using eCommerce.Application.DTOs.Product;
using eCommerce.Application.Common;

namespace eCommerce.Application.Interfaces;

public interface IProductService
{
    Task<Result<IEnumerable<ProductResponseDto>>> GetAllProductsAsync();
    Task<Result<IEnumerable<ProductResponseDto>>> GetAllProductsAsync(string? search = default, string? category = default, decimal? minPrice = default, decimal? maxPrice = default, int pageNumber = 1, int pageSize = 10);
    Task<Result<ProductResponseDto>> GetProductByIdAsync(Guid id);

    Task<Result<ProductResponseDto>> CreateProductAsync(CreateProductDto productCreateDto);
    Task<Result<ProductResponseDto>> UpdateProductAsync(Guid id, UpdateProductDto productUpdateDto);
    Task<Result<bool>> DeleteProductAsync(Guid id);
}
