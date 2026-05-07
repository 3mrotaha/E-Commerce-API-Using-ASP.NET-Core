using eCommerce.Application.DTOs.ProductCategory;
using eCommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Api.Controllers.V1
{
    public class CategoriesController : CustomControllerBase
    {
        private readonly IProductCategoryService _productCategoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(IProductCategoryService productCategoryService, ILogger<CategoriesController> logger)
        {
            _productCategoryService = productCategoryService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,SUPER_ADMIN", AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> AddCategory([FromBody] CreateProductCategoryDto createProductCategoryDto)
        {
            var category = await _productCategoryService.CreateCategoryAsync(createProductCategoryDto);
            return ToActionResult(category);
        }

        [HttpPut("{categoryId:guid}")]
        [Authorize(Roles = "ADMIN,SUPER_ADMIN", AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> UpdateCategory([FromRoute] Guid categoryId, [FromBody] UpdateProductCategoryDto updateProductCategoryDto)
        {
            var category = await _productCategoryService.UpdateCategoryAsync(categoryId, updateProductCategoryDto);
            return ToActionResult(category);
        }

        [HttpDelete("{categoryId:guid}")]
        [Authorize(Roles = "ADMIN,SUPER_ADMIN", AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> DeleteCategory([FromRoute] Guid categoryId)
        {
            var isDeleted = await _productCategoryService.DeleteCategoryAsync(categoryId);
            return ToMessageActionResult(isDeleted, $"Category with id={categoryId} deleted successfully");
        }

        [HttpGet]
        [Authorize(Roles = "USER,ADMIN,SUPER_ADMIN", AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _productCategoryService.GetAllCategoriesAsync();
            return ToActionResult(categories);
        }
    }
}
