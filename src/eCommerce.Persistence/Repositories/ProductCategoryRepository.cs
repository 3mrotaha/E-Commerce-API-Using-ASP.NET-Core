using System.Linq.Expressions;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using eCommerce.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eCommerce.Persistence.Repositories;

public class ProductCategoryRepository : IProductCategoryRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductCategoryRepository> _logger;

    public ProductCategoryRepository(AppDbContext context, ILogger<ProductCategoryRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProductCategory?> AddAsync(ProductCategory entity)
    {
        try
        {
            _logger.LogDebug("{Repository} - Executing repository operation", nameof(ProductCategoryRepository));
            _context.Set<ProductCategory>().Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully added product category {Entity}", nameof(ProductCategoryRepository), nameof(AddAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error adding product category {Entity}", nameof(ProductCategoryRepository), nameof(AddAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(ProductCategoryRepository));
            throw;
        }
    }

    public async Task DeleteAsync(ProductCategory entity)
    {
        try
        {
            _logger.LogDebug("{Repository} - Executing repository operation", nameof(ProductCategoryRepository));
            _context.Set<ProductCategory>().Remove(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully deleted product category {Entity}", nameof(ProductCategoryRepository), nameof(DeleteAsync), entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error deleting product category {Entity}", nameof(ProductCategoryRepository), nameof(DeleteAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(ProductCategoryRepository));
            throw;
        }
    }

    public async Task<IEnumerable<ProductCategory>> FindAsync(Expression<Func<ProductCategory, bool>> predicate)
    {
        try
        {
            _logger.LogDebug("{Repository} - Executing repository operation", nameof(ProductCategoryRepository));
            var categories = _context.Set<ProductCategory>().AsNoTracking()
                                                            .Where(predicate)
                                                            .AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully found product categories with predicate {Predicate}", nameof(ProductCategoryRepository), nameof(FindAsync), predicate);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error finding product categories with predicate {Predicate}", nameof(ProductCategoryRepository), nameof(FindAsync), predicate);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(ProductCategoryRepository));
            throw;
        }
    }

    public async Task<IEnumerable<ProductCategory>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug("{Repository} - Executing repository operation", nameof(ProductCategoryRepository));
            var categories = _context.Set<ProductCategory>().AsNoTracking().AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved all product categories", nameof(ProductCategoryRepository), nameof(GetAllAsync));
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving all product categories", nameof(ProductCategoryRepository), nameof(GetAllAsync));
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(ProductCategoryRepository));
            throw;
        }
    }

    public async Task<ProductCategory?> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogDebug("{Repository} - Executing repository operation", nameof(ProductCategoryRepository));
            var category = await _context.Set<ProductCategory>().AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved product category by id {Id}", nameof(ProductCategoryRepository), nameof(GetByIdAsync), id);
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving product category by id {Id}", nameof(ProductCategoryRepository), nameof(GetByIdAsync), id);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(ProductCategoryRepository));
            throw;
        }
    }

    public async Task<ProductCategory?> UpdateAsync(ProductCategory entity)
    {
        try
        {
            _logger.LogDebug("{Repository} - Executing repository operation", nameof(ProductCategoryRepository));
            _context.Set<ProductCategory>().Update(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully updated product category {Entity}", nameof(ProductCategoryRepository), nameof(UpdateAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error updating product category {Entity}", nameof(ProductCategoryRepository), nameof(UpdateAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(ProductCategoryRepository));
            throw;
        }
    }
}



