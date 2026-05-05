using eCommerce.Api.Controllers;
using eCommerce.Application.DTOs.Product;
using eCommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Api.Controllers.V1
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
    public class ProductsController : CustomControllerBase
    {
        private readonly IProductService _productService;
        private readonly IProductCategoryService _productCategoryService;
        public ProductsController(IProductService productService, IProductCategoryService productCategoryService)
        {
            _productService = productService;
            _productCategoryService = productCategoryService;
        }

        [HttpGet("{pageNumber:int}/{pageSize:int:max(100)}")]
        public async Task<IActionResult> GetProducts([FromRoute] int pageNumber,
                                                    [FromRoute] int pageSize,
                                                    [FromQuery] string? search,
                                                    [FromQuery] string? category,
                                                    [FromQuery] decimal? minPrice,
                                                    [FromQuery] decimal? maxPrice)
        {
            var products = await _productService.GetAllProductsAsync(search, category, minPrice, maxPrice, pageNumber, pageSize);
            return ToActionResult(products);
        }

        [HttpGet("{productId:guid}")]
        public async Task<IActionResult> GetProduct(Guid productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            return ToActionResult(product);
        }

        [HttpGet("category/{categoryId:guid}")]
        public async Task<IActionResult> GetProductsByCategory(Guid categoryId)
        {
            var category = await _productCategoryService.GetCategoryByIdAsync(categoryId);
            if (category.IsFailure)
            {
                return ToActionResult(category);
            }

            var products = await _productService.GetAllProductsAsync(category: category.Value!.Name);

            return ToActionResult(products);
        }


        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPER_ADMIN", AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> AddProduct([FromBody] CreateProductDto product)
        {
            var result = await _productService.CreateProductAsync(product);
            return ToActionResult(result);
        }

        [HttpPut("{productId:guid}")]
        [Authorize(Roles = "ADMIN,SUPER_ADMIN", AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> UpdateProduct([FromRoute] Guid productId, [FromBody] UpdateProductDto productUpdate)
        {
            var productUpdated = await _productService.UpdateProductAsync(productId, productUpdate);
            return ToActionResult(productUpdated);
        }

        [HttpDelete("{productId:guid}")]
        [Authorize(Roles = "ADMIN,SUPER_ADMIN", AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> DeleteProduct([FromRoute] Guid productId)
        {
            var isDeleted = await _productService.DeleteProductAsync(productId);
            return ToMessageActionResult(isDeleted, $"Product with Id={productId} Deleted successfully");
        }
    }
}
