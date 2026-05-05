using System.Linq.Expressions;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using eCommerce.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eCommerce.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(AppDbContext context, ILogger<OrderRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Order?> AddAsync(Order entity)
    {
        try
        {
            _context.Orders.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully added order {Entity}", nameof(OrderRepository), nameof(AddAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error adding order {Entity}", nameof(OrderRepository), nameof(AddAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(OrderRepository));
            throw;
        }
    }

    public async Task DeleteAsync(Order entity)
    {
        try
        {
            _context.Orders.Remove(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully deleted order {Entity}", nameof(OrderRepository), nameof(DeleteAsync), entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error deleting order {Entity}", nameof(OrderRepository), nameof(DeleteAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(OrderRepository));
            throw;
        }
    }

    public async Task<IEnumerable<Order>> FindAsync(Expression<Func<Order, bool>> predicate)
    {
        try
        {
            var orders = _context.Orders.AsNoTracking()
                                        .Where(predicate)
                                        .AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully found orders with predicate {Predicate}", nameof(OrderRepository), nameof(FindAsync), predicate);
            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error finding orders with predicate {Predicate}", nameof(OrderRepository), nameof(FindAsync), predicate);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(OrderRepository));
            throw;
        }
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        try
        {
            var orders = _context.Orders.AsNoTracking().AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved all orders", nameof(OrderRepository), nameof(GetAllAsync));
            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving all orders", nameof(OrderRepository), nameof(GetAllAsync));
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(OrderRepository));
            throw;
        }
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        try
        {
            var order = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved order by id {Id}", nameof(OrderRepository), nameof(GetByIdAsync), id);
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving order by id {Id}", nameof(OrderRepository), nameof(GetByIdAsync), id);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(OrderRepository));
            throw;
        }
    }

    public async Task<Order?> UpdateAsync(Order entity)
    {
        try
        {
            _context.Orders.Update(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully updated order {Entity}", nameof(OrderRepository), nameof(UpdateAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error updating order {Entity}", nameof(OrderRepository), nameof(UpdateAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(OrderRepository));
            throw;
        }
    }
}



