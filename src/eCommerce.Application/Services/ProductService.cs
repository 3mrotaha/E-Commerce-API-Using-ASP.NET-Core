using System.Text.Json;
using AutoMapper;
using eCommerce.Application.Common;
using eCommerce.Application.DTOs.Product;
using eCommerce.Application.Interfaces;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace eCommerce.Application.Services;

public class ProductService : IProductService
{
    // Matches SQL decimal(18,2) upper bound used by Product.UnitPrice.
    private const decimal MaxUnitPriceFilter = 9999999999999999.99m;
    private readonly IDistributedCache _cache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductService> logger, IDistributedCache cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<ProductResponseDto>> CreateProductAsync(CreateProductDto productCreateDto)
    {
        _logger.LogDebug("Creating product {ProductName} with category {CategoryId}", productCreateDto.Name, productCreateDto.CategoryId);

        if (productCreateDto.CategoryId.HasValue)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(productCreateDto.CategoryId.Value);
            if (category == null)
            {
                _logger.LogError("Cannot create product {ProductName}; category {CategoryId} was not found", productCreateDto.Name, productCreateDto.CategoryId);
                return Result<ProductResponseDto>.NotFound($"Category with id '{productCreateDto.CategoryId}' was not found.");
            }
        }

        var product = _mapper.Map<Product>(productCreateDto);
        product.Id = product.Id == Guid.Empty ? Guid.NewGuid() : product.Id;

        var createdProduct = await _unitOfWork.Products.AddAsync(product);
        if (createdProduct == null)
        {
            _logger.LogError("Failed to create product {ProductName}", productCreateDto.Name);
            return Result<ProductResponseDto>.BadRequest("Failed to create product.");
        }

        var productWithCategory = (await _unitOfWork.Products.FindAsync(p => p.Id == createdProduct.Id)).FirstOrDefault();
        _logger.LogInformation("Created product {ProductId} named {ProductName}", createdProduct.Id, createdProduct.Name);
        return Result<ProductResponseDto>.Success(_mapper.Map<ProductResponseDto>(productWithCategory ?? createdProduct));
    }

    public async Task<Result<bool>> DeleteProductAsync(Guid id)
    {
        _logger.LogDebug("Deleting product {ProductId}", id);

        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
        {
            _logger.LogError("Cannot delete product {ProductId}; product was not found", id);
            return Result<bool>.NotFound($"Product with id '{id}' was not found.");
        }

        await _unitOfWork.Products.DeleteAsync(product);

        // cache invalidation
        await _cache.RemoveAsync($"product:{id}");
        
        _logger.LogInformation("Deleted product {ProductId}", id);
        return Result<bool>.Success(true);
    }

    public async Task<Result<IEnumerable<ProductResponseDto>>> GetAllProductsAsync()
    {
        _logger.LogDebug("Getting all products");

        var cachedProductsJson = await _cache.GetStringAsync("products:all");

        if(cachedProductsJson == null)
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            cachedProductsJson = JsonSerializer.Serialize(_mapper.Map<List<ProductResponseDto>>(products));
            await _cache.SetStringAsync("products:all", cachedProductsJson);
        }

        var cachedProducts = JsonSerializer.Deserialize<List<ProductResponseDto>>(cachedProductsJson);
        _logger.LogInformation("Retrieved {ProductCount} products", cachedProducts?.Count ?? 0);
        return Result<IEnumerable<ProductResponseDto>>.Success(cachedProducts ?? []);
    }

    public async Task<Result<IEnumerable<ProductResponseDto>>> GetAllProductsAsync(string? search, string? category, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize)
    {
        _logger.LogDebug(
            "Getting products with search {Search}, category {Category}, min price {MinPrice}, max price {MaxPrice}, page {PageNumber}, page size {PageSize}",
            search,
            category,
            minPrice,
            maxPrice,
            pageNumber,
            pageSize);

        List<Product>? products;
        var minPriceValue = Math.Max(minPrice ?? 0m, 0m);
        var maxPriceValue = maxPrice ?? MaxUnitPriceFilter;
        maxPriceValue = Math.Min(maxPriceValue, MaxUnitPriceFilter);

        // form cache key
        string cacheKey=$"products:all:{search??"na"}-{category??"na"}-{minPriceValue}-{maxPriceValue}-{pageNumber}-{pageSize}";
        cacheKey = cacheKey.Replace(".","-"); // replace the dot in decimal values to avoid issues with cache key parsing
        
        // get from cache
        var cachedProductsJson = await _cache.GetStringAsync(cacheKey);

        // cache miss
        if(cachedProductsJson == null)
        {            
            if (search == null)
            {
                if (category == null)
                {
                    products = (await _unitOfWork.Products.FindAsync(p => p.UnitPrice >= minPriceValue && p.UnitPrice <= maxPriceValue)).ToList();
                }
                else
                {
                    products = (await _unitOfWork.Products.FindAsync(p => p.Category != null && p.Category.Name == category && p.UnitPrice >= minPriceValue && p.UnitPrice <= maxPriceValue)).ToList();
                }
            }
            else
            {
                if (category == null)
                {
                    products = (await _unitOfWork.Products.FindAsync(p => p.Name.Contains(search) && p.UnitPrice >= minPriceValue && p.UnitPrice <= maxPriceValue)).ToList();
                }
                else
                {
                    products = (await _unitOfWork.Products.FindAsync(p => p.Name.Contains(search) && p.Category != null && p.Category.Name == category && p.UnitPrice >= minPriceValue && p.UnitPrice <= maxPriceValue)).ToList();
                }
            }

            var paginatedProduct = products.Skip((pageNumber - 1) * pageSize)
                                        .Take(pageSize);

            _logger.LogInformation("Retrieved {ProductCount} products for page {PageNumber}", paginatedProduct.Count(), pageNumber);
            
            var responseResult = _mapper.Map<List<ProductResponseDto>>(paginatedProduct);

            // update cache
            cachedProductsJson = JsonSerializer.Serialize(responseResult);
            await _cache.SetStringAsync(cacheKey, cachedProductsJson);
            
            return Result<IEnumerable<ProductResponseDto>>.Success(responseResult);
        }

        // cache hit        
        var cachedProducts = JsonSerializer.Deserialize<List<ProductResponseDto>>(cachedProductsJson);
        _logger.LogInformation("Retrieved {ProductCount} products from cache for page {PageNumber}", cachedProducts?.Count ?? 0, pageNumber);

        return Result<IEnumerable<ProductResponseDto>>.Success(cachedProducts ?? []);
    }

    public async Task<Result<ProductResponseDto>> GetProductByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting product {ProductId}", id);

        string key = $"product:{id}";
        var cachedProductJson = await _cache.GetStringAsync(key);
        
        // cache miss
        if(cachedProductJson == null)
        {
            var product = (await _unitOfWork.Products.FindAsync(p => p.Id == id)).FirstOrDefault();
            
            if (product == null)
            {
                _logger.LogError("Product {ProductId} was not found", id);
                return Result<ProductResponseDto>.NotFound($"Product with id '{id}' was not found.");
            }

            cachedProductJson = JsonSerializer.Serialize(_mapper.Map<ProductResponseDto>(product));

            // store to cache
            await _cache.SetStringAsync(key, cachedProductJson);
        }

        // cache hit
        var cachedProduct = JsonSerializer.Deserialize<ProductResponseDto>(cachedProductJson);
        if (cachedProduct is null)
        {
            _logger.LogCritical("Cached product {ProductId} could not be deserialized", id);
        }
        else
        {
            _logger.LogInformation("Retrieved product {ProductId}", id);
        }

        return cachedProduct is null
            ? Result<ProductResponseDto>.Failure("Failed to deserialize cached product.")
            : Result<ProductResponseDto>.Success(cachedProduct);
    }

    public async Task<Result<ProductResponseDto>> UpdateProductAsync(Guid id, UpdateProductDto productUpdateDto)
    {
        _logger.LogDebug("Updating product {ProductId}", id);

        var existingProduct = await _unitOfWork.Products.GetByIdAsync(id);
        if (existingProduct == null)
        {
            _logger.LogError("Cannot update product {ProductId}; product was not found", id);
            return Result<ProductResponseDto>.NotFound($"Product with id '{id}' was not found.");
        }

        if (productUpdateDto.CategoryId.HasValue)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(productUpdateDto.CategoryId.Value);
            if (category == null)
            {
                _logger.LogError("Cannot update product {ProductId}; category {CategoryId} was not found", id, productUpdateDto.CategoryId);
                return Result<ProductResponseDto>.NotFound($"Category with id '{productUpdateDto.CategoryId}' was not found.");
            }
        }

        existingProduct.CategoryId = productUpdateDto.CategoryId;
        existingProduct.Name = productUpdateDto.Name;
        existingProduct.Description = productUpdateDto.Description;
        existingProduct.QuantityInStock = productUpdateDto.QuantityInStock;
        existingProduct.UnitPrice = productUpdateDto.UnitPrice;
        existingProduct.UpdatedAt = DateTime.UtcNow;

        var updatedProduct = await _unitOfWork.Products.UpdateAsync(existingProduct);
        if (updatedProduct == null)
        {
            _logger.LogError("Failed to update product with id {ProductId}", id);
            return Result<ProductResponseDto>.BadRequest("Failed to update product.");
        }

        var productWithCategory = (await _unitOfWork.Products.FindAsync(p => p.Id == updatedProduct.Id)).FirstOrDefault();
        
        // cache invalidation
        await _cache.RemoveAsync($"product:{id}");

        _logger.LogInformation("Updated product {ProductId}", id);
        return Result<ProductResponseDto>.Success(_mapper.Map<ProductResponseDto>(productWithCategory ?? updatedProduct));
    }

}
