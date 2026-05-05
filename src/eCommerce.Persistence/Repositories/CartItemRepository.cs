using System.Linq.Expressions;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using eCommerce.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eCommerce.Persistence.Repositories;

public class CartItemRepository : ICartItemRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<CartItemRepository> _logger;

    public CartItemRepository(AppDbContext context, ILogger<CartItemRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CartItem?> AddAsync(CartItem entity)
    {
        try
        {
            _context.CartItems.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully added cart item {Entity}", nameof(CartItemRepository), nameof(AddAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error adding cart item {Entity}", nameof(CartItemRepository), nameof(AddAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(CartItemRepository));
            throw;
        }
    }

    public async Task DeleteAsync(CartItem entity)
    {
        try
        {
            _context.CartItems.Remove(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully deleted cart item {Entity}", nameof(CartItemRepository), nameof(DeleteAsync), entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error deleting cart item {Entity}", nameof(CartItemRepository), nameof(DeleteAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(CartItemRepository));
            throw;
        }
    }

    public async Task<IEnumerable<CartItem>> FindAsync(Expression<Func<CartItem, bool>> predicate)
    {
        try
        {
            var cartItems = _context.CartItems.AsNoTracking()
                                              .Where(predicate)
                                              .AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully found cart items with predicate {Predicate}", nameof(CartItemRepository), nameof(FindAsync), predicate);
            return cartItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error finding cart items with predicate {Predicate}", nameof(CartItemRepository), nameof(FindAsync), predicate);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(CartItemRepository));
            throw;
        }
    }

    public async Task<IEnumerable<CartItem>> GetAllAsync()
    {
        try
        {
            var cartItems = _context.CartItems.AsNoTracking().AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved all cart items", nameof(CartItemRepository), nameof(GetAllAsync));
            return cartItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving all cart items", nameof(CartItemRepository), nameof(GetAllAsync));
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(CartItemRepository));
            throw;
        }
    }

    public Task<CartItem?> GetByIdAsync(Guid id)
    {
        _logger.LogWarning("{Repository}-{Method} - CartItem uses an int primary key; lookup by Guid is not supported", nameof(CartItemRepository), nameof(GetByIdAsync));
        return Task.FromResult<CartItem?>(null);
    }

    public async Task<CartItem?> UpdateAsync(CartItem entity)
    {
        try
        {
            _context.CartItems.Update(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully updated cart item {Entity}", nameof(CartItemRepository), nameof(UpdateAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error updating cart item {Entity}", nameof(CartItemRepository), nameof(UpdateAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(CartItemRepository));
            throw;
        }
    }
}



