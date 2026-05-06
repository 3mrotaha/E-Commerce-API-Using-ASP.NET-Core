using System.Linq.Expressions;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using eCommerce.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eCommerce.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(AppDbContext context, ILogger<ProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Product?> AddAsync(Product entity)
    {
        try
        {
            _logger.LogDebug("{Repository} - Executing repository operation", nameof(ProductRepository));
            _context.Set<Product>().Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully added product {Entity}", nameof(ProductRepository), nameof(AddAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error adding product {Entity}", nameof(ProductRepository), nameof(AddAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(ProductRepository));
            throw;
        }
    }

    public async Task DeleteAsync(Product entity)
    {
        try
        {
            _logger.LogDebug("{Repository} - Executing repository operation", nameof(ProductRepository));
            _context.Set<Product>().Remove(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully deleted product {Entity}", nameof(ProductRepository), nameof(DeleteAsync), entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error deleting product {Entity}", nameof(ProductRepository), nameof(DeleteAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(ProductRepository));
            throw;
        }
    }

    public async Task<IEnumerable<Product>> FindAsync(Expression<Func<Product, bool>> predicate)
    {
        try
        {
            _logger.LogDebug("{Repository} - Executing repository operation", nameof(ProductRepository));
            var products = await _context.Set<Product>().AsNoTracking()
                                                  .Include(p => p.Category)
                                                  .Where(predicate)
                                                  .ToListAsync();

            _logger.LogInformation("{Repository}-{Method} - Successfully found products with predicate {Predicate}", nameof(ProductRepository), nameof(FindAsync), predicate);
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error finding products with predicate {Predicate}", nameof(ProductRepository), nameof(FindAsync), predicate);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(ProductRepository));
            throw;
        }
    }

    public async Task<IEnumerable<Product>> FindAsync(Expression<Func<Product, bool>> predicate, int pageNumber, int pageSize)
    {
        try
        {
            _logger.LogDebug("{Repository} - Executing repository operation", nameof(ProductRepository));
            var products = await _context.Set<Product>().AsNoTracking()
                                                  .Where(predicate)
                                                  .OrderBy(p => p.Id)
                                                  .Skip((pageNumber - 1) * pageSize)
                                                  .Take(pageSize)
                                                  .ToListAsync();

            _logger.LogInformation("{Repository}-{Method} - Successfully found products with predicate {Predicate}", nameof(ProductRepository), nameof(FindAsync), predicate);
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error finding products with predicate {Predicate}", nameof(ProductRepository), nameof(FindAsync), predicate);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(ProductRepository));
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug("{Repository} - Executing repository operation", nameof(ProductRepository));
            var products = _context.Set<Product>().AsNoTracking().AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved all products", nameof(ProductRepository), nameof(GetAllAsync));
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving all products", nameof(ProductRepository), nameof(GetAllAsync));
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(ProductRepository));
            throw;
        }
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogDebug("{Repository} - Executing repository operation", nameof(ProductRepository));
            var product = await _context.Set<Product>().AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved product by id {Id}", nameof(ProductRepository), nameof(GetByIdAsync), id);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving product by id {Id}", nameof(ProductRepository), nameof(GetByIdAsync), id);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(ProductRepository));
            throw;
        }
    }

    public async Task<Product?> UpdateAsync(Product entity)
    {
        try
        {
            _logger.LogDebug("{Repository} - Executing repository operation", nameof(ProductRepository));
            _context.Set<Product>().Update(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully updated product {Entity}", nameof(ProductRepository), nameof(UpdateAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error updating product {Entity}", nameof(ProductRepository), nameof(UpdateAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(ProductRepository));
            throw;
        }
    }
}



