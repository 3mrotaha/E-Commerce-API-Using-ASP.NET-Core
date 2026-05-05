using AutoMapper;
using eCommerce.Application.Common;
using eCommerce.Application.DTOs.ProductCategory;
using eCommerce.Application.Interfaces;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace eCommerce.Application.Services;

public class ProductCategoryService : IProductCategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductCategoryService> _logger;

    public ProductCategoryService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductCategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ProductCategoryResponseDto>> CreateCategoryAsync(CreateProductCategoryDto categoryCreateDto)
    {
        _logger.LogDebug("Creating category {CategoryName}", categoryCreateDto.Name);

        var existingCategory = (await _unitOfWork.Categories.FindAsync(c => c.Name == categoryCreateDto.Name)).FirstOrDefault();
        if (existingCategory != null)
        {
            _logger.LogError("Cannot create category {CategoryName}; category already exists", categoryCreateDto.Name);
            return Result<ProductCategoryResponseDto>.Conflict($"Category with name '{categoryCreateDto.Name}' already exists.");
        }

        var category = _mapper.Map<ProductCategory>(categoryCreateDto);
        category.Id = Guid.NewGuid();

        var createdCategory = await _unitOfWork.Categories.AddAsync(category);
        if (createdCategory == null)
        {
            _logger.LogError("Failed to create category {CategoryName}", categoryCreateDto.Name);
            _logger.LogCritical("Category repository returned null while creating category {CategoryName}", categoryCreateDto.Name);
            return Result<ProductCategoryResponseDto>.BadRequest("Failed to create category.");
        }

        _logger.LogInformation("Created category {CategoryId} named {CategoryName}", createdCategory.Id, createdCategory.Name);
        return Result<ProductCategoryResponseDto>.Success(_mapper.Map<ProductCategoryResponseDto>(createdCategory));
    }

    public async Task<Result<bool>> DeleteCategoryAsync(Guid id)
    {
        _logger.LogDebug("Deleting category {CategoryId}", id);

        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
        {
            _logger.LogError("Cannot delete category {CategoryId}; category was not found", id);
            return Result<bool>.NotFound($"Category with id '{id}' was not found.");
        }

        await _unitOfWork.Categories.DeleteAsync(category);
        _logger.LogInformation("Deleted category {CategoryId}", id);
        return Result<bool>.Success(true);
    }

    public async Task<Result<IEnumerable<ProductCategoryResponseDto>>> GetAllCategoriesAsync()
    {
        _logger.LogDebug("Getting all categories");

        var categories = await _unitOfWork.Categories.GetAllAsync();
        _logger.LogInformation("Retrieved categories");
        return Result<IEnumerable<ProductCategoryResponseDto>>.Success(_mapper.Map<IEnumerable<ProductCategoryResponseDto>>(categories));
    }

    public async Task<Result<ProductCategoryResponseDto>> GetCategoryByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting category {CategoryId}", id);

        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
        {
            _logger.LogError("Category {CategoryId} was not found", id);
            return Result<ProductCategoryResponseDto>.NotFound($"Category with id '{id}' was not found.");
        }

        _logger.LogInformation("Retrieved category {CategoryId}", id);
        return Result<ProductCategoryResponseDto>.Success(_mapper.Map<ProductCategoryResponseDto>(category));
    }

    public async Task<Result<ProductCategoryResponseDto>> UpdateCategoryAsync(Guid id, UpdateProductCategoryDto categoryUpdateDto)
    {
        _logger.LogDebug("Updating category {CategoryId}", id);

        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
        {
            _logger.LogError("Cannot update category {CategoryId}; category was not found", id);
            return Result<ProductCategoryResponseDto>.NotFound($"Category with id '{id}' was not found.");
        }

        var categoryWithSameName = (await _unitOfWork.Categories.FindAsync(c => c.Name == categoryUpdateDto.Name && c.Id != id)).FirstOrDefault();
        if (categoryWithSameName != null)
        {
            _logger.LogError("Cannot update category {CategoryId}; category name {CategoryName} already exists", id, categoryUpdateDto.Name);
            return Result<ProductCategoryResponseDto>.Conflict($"Category with name '{categoryUpdateDto.Name}' already exists.");
        }

        category.Name = categoryUpdateDto.Name;
        category.Description = categoryUpdateDto.Description;

        var updatedCategory = await _unitOfWork.Categories.UpdateAsync(category);
        if (updatedCategory == null)
        {
            _logger.LogError("Failed to update category with id {CategoryId}", id);
            _logger.LogCritical("Category repository returned null while updating category {CategoryId}", id);
            return Result<ProductCategoryResponseDto>.BadRequest("Failed to update category.");
        }

        _logger.LogInformation("Updated category {CategoryId}", id);
        return Result<ProductCategoryResponseDto>.Success(_mapper.Map<ProductCategoryResponseDto>(updatedCategory));
    }
}
