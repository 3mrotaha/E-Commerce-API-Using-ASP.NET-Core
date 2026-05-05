using System.Linq.Expressions;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using eCommerce.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eCommerce.Persistence.Repositories;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrderItemRepository> _logger;

    public OrderItemRepository(AppDbContext context, ILogger<OrderItemRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OrderItem?> AddAsync(OrderItem entity)
    {
        try
        {
            _context.OrderItems.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully added order item {Entity}", nameof(OrderItemRepository), nameof(AddAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error adding order item {Entity}", nameof(OrderItemRepository), nameof(AddAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(OrderItemRepository));
            throw;
        }
    }

    public async Task DeleteAsync(OrderItem entity)
    {
        try
        {
            _context.OrderItems.Remove(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully deleted order item {Entity}", nameof(OrderItemRepository), nameof(DeleteAsync), entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error deleting order item {Entity}", nameof(OrderItemRepository), nameof(DeleteAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(OrderItemRepository));
            throw;
        }
    }

    public async Task<IEnumerable<OrderItem>> FindAsync(Expression<Func<OrderItem, bool>> predicate)
    {
        try
        {
            var orderItems = _context.OrderItems.AsNoTracking()
                                                .Where(predicate)
                                                .AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully found order items with predicate {Predicate}", nameof(OrderItemRepository), nameof(FindAsync), predicate);
            return orderItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error finding order items with predicate {Predicate}", nameof(OrderItemRepository), nameof(FindAsync), predicate);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(OrderItemRepository));
            throw;
        }
    }

    public async Task<IEnumerable<OrderItem>> GetAllAsync()
    {
        try
        {
            var orderItems = _context.OrderItems.AsNoTracking().AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved all order items", nameof(OrderItemRepository), nameof(GetAllAsync));
            return orderItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving all order items", nameof(OrderItemRepository), nameof(GetAllAsync));
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(OrderItemRepository));
            throw;
        }
    }

    public Task<OrderItem?> GetByIdAsync(Guid id)
    {
        _logger.LogWarning("{Repository}-{Method} - OrderItem uses an int primary key; lookup by Guid is not supported", nameof(OrderItemRepository), nameof(GetByIdAsync));
        return Task.FromResult<OrderItem?>(null);
    }

    public async Task<OrderItem?> UpdateAsync(OrderItem entity)
    {
        try
        {
            _context.OrderItems.Update(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully updated order item {Entity}", nameof(OrderItemRepository), nameof(UpdateAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error updating order item {Entity}", nameof(OrderItemRepository), nameof(UpdateAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(OrderItemRepository));
            throw;
        }
    }
}



