using eCommerce.Application.DTOs.ProductCategory;
using eCommerce.Application.Common;

namespace eCommerce.Application.Interfaces;

public interface IProductCategoryService
{
    Task<Result<IEnumerable<ProductCategoryResponseDto>>> GetAllCategoriesAsync();
    Task<Result<ProductCategoryResponseDto>> GetCategoryByIdAsync(Guid id);
    Task<Result<ProductCategoryResponseDto>> CreateCategoryAsync(CreateProductCategoryDto categoryCreateDto);
    Task<Result<ProductCategoryResponseDto>> UpdateCategoryAsync(Guid id, UpdateProductCategoryDto categoryUpdateDto);
    Task<Result<bool>> DeleteCategoryAsync(Guid id);
}
