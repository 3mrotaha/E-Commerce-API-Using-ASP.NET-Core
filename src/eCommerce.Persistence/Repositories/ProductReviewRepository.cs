using System.Linq.Expressions;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using eCommerce.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eCommerce.Persistence.Repositories;

public class ProductReviewRepository : IProductReviewRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductReviewRepository> _logger;

    public ProductReviewRepository(AppDbContext context, ILogger<ProductReviewRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProductReview?> AddAsync(ProductReview entity)
    {
        try
        {
            _logger.LogDebug("{Repository} - Executing repository operation", nameof(ProductReviewRepository));
            _context.ProductReviews.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully added product review {Entity}", nameof(ProductReviewRepository), nameof(AddAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error adding product review {Entity}", nameof(ProductReviewRepository), nameof(AddAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(ProductReviewRepository));
            throw;
        }
    }

    public async Task DeleteAsync(ProductReview entity)
    {
        try
        {
            _logger.LogDebug("{Repository} - Executing repository operation", nameof(ProductReviewRepository));
            _context.ProductReviews.Remove(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully deleted product review {Entity}", nameof(ProductReviewRepository), nameof(DeleteAsync), entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error deleting product review {Entity}", nameof(ProductReviewRepository), nameof(DeleteAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(ProductReviewRepository));
            throw;
        }
    }

    public async Task<IEnumerable<ProductReview>> FindAsync(Expression<Func<ProductReview, bool>> predicate)
    {
        try
        {
            _logger.LogDebug("{Repository} - Executing repository operation", nameof(ProductReviewRepository));
            var reviews = _context.ProductReviews.AsNoTracking()
                                                 .Where(predicate)
                                                 .AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully found product reviews with predicate {Predicate}", nameof(ProductReviewRepository), nameof(FindAsync), predicate);
            return reviews;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error finding product reviews with predicate {Predicate}", nameof(ProductReviewRepository), nameof(FindAsync), predicate);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(ProductReviewRepository));
            throw;
        }
    }

    public async Task<IEnumerable<ProductReview>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug("{Repository} - Executing repository operation", nameof(ProductReviewRepository));
            var reviews = _context.ProductReviews.AsNoTracking().AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved all product reviews", nameof(ProductReviewRepository), nameof(GetAllAsync));
            return reviews;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving all product reviews", nameof(ProductReviewRepository), nameof(GetAllAsync));
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(ProductReviewRepository));
            throw;
        }
    }

    public async Task<ProductReview?> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogDebug("{Repository} - Executing repository operation", nameof(ProductReviewRepository));
            var review = await _context.ProductReviews.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved product review by id {Id}", nameof(ProductReviewRepository), nameof(GetByIdAsync), id);
            return review;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving product review by id {Id}", nameof(ProductReviewRepository), nameof(GetByIdAsync), id);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(ProductReviewRepository));
            throw;
        }
    }

    public async Task<ProductReview?> UpdateAsync(ProductReview entity)
    {
        try
        {
            _logger.LogDebug("{Repository} - Executing repository operation", nameof(ProductReviewRepository));
            _context.ProductReviews.Update(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully updated product review {Entity}", nameof(ProductReviewRepository), nameof(UpdateAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error updating product review {Entity}", nameof(ProductReviewRepository), nameof(UpdateAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(ProductReviewRepository));
            throw;
        }
    }
}



