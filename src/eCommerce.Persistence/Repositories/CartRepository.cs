using System.Linq.Expressions;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using eCommerce.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eCommerce.Persistence.Repositories;

public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<CartRepository> _logger;

    public CartRepository(AppDbContext context, ILogger<CartRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Cart?> AddAsync(Cart entity)
    {
        try
        {
            _context.Carts.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully added cart {Entity}", nameof(CartRepository), nameof(AddAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error adding cart {Entity}", nameof(CartRepository), nameof(AddAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(CartRepository));
            throw;
        }
    }

    public async Task DeleteAsync(Cart entity)
    {
        try
        {
            _context.Carts.Remove(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully deleted cart {Entity}", nameof(CartRepository), nameof(DeleteAsync), entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error deleting cart {Entity}", nameof(CartRepository), nameof(DeleteAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(CartRepository));
            throw;
        }
    }

    public async Task<IEnumerable<Cart>> FindAsync(Expression<Func<Cart, bool>> predicate)
    {
        try
        {
            var carts = _context.Carts.AsNoTracking()
                                      .Where(predicate)
                                      .AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully found carts with predicate {Predicate}", nameof(CartRepository), nameof(FindAsync), predicate);
            return carts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error finding carts with predicate {Predicate}", nameof(CartRepository), nameof(FindAsync), predicate);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(CartRepository));
            throw;
        }
    }

    public async Task<IEnumerable<Cart>> GetAllAsync()
    {
        try
        {
            var carts = _context.Carts.AsNoTracking().AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved all carts", nameof(CartRepository), nameof(GetAllAsync));
            return carts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving all carts", nameof(CartRepository), nameof(GetAllAsync));
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(CartRepository));
            throw;
        }
    }

    public async Task<Cart?> GetByIdAsync(Guid id)
    {
        try
        {
            var cart = await _context.Carts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved cart by id {Id}", nameof(CartRepository), nameof(GetByIdAsync), id);
            return cart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving cart by id {Id}", nameof(CartRepository), nameof(GetByIdAsync), id);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(CartRepository));
            throw;
        }
    }

    public async Task<Cart?> UpdateAsync(Cart entity)
    {
        try
        {
            _context.Carts.Update(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully updated cart {Entity}", nameof(CartRepository), nameof(UpdateAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error updating cart {Entity}", nameof(CartRepository), nameof(UpdateAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(CartRepository));
            throw;
        }
    }
}



