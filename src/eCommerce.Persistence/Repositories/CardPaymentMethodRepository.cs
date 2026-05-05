using System.Linq.Expressions;
using eCommerce.Domain;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using eCommerce.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eCommerce.Persistence.Repositories;

public class CardPaymentMethodRepository : ICardPaymentMethodRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<CardPaymentMethodRepository> _logger;

    public CardPaymentMethodRepository(AppDbContext context, ILogger<CardPaymentMethodRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CardPaymentMethod?> AddAsync(CardPaymentMethod entity)
    {
        try
        {
            _context.PaymentMethods.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully added card payment method {Entity}", nameof(CardPaymentMethodRepository), nameof(IGenericRepository<CardPaymentMethod>.AddAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error adding card payment method {Entity}", nameof(CardPaymentMethodRepository), nameof(IGenericRepository<CardPaymentMethod>.AddAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(CardPaymentMethodRepository));
            throw;
        }
    }

    public async Task DeleteAsync(CardPaymentMethod entity)
    {
        try
        {
            _context.PaymentMethods.Remove(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully deleted card payment method {Entity}", nameof(CardPaymentMethodRepository), nameof(IGenericRepository<CardPaymentMethod>.DeleteAsync), entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error deleting card payment method {Entity}", nameof(CardPaymentMethodRepository), nameof(IGenericRepository<CardPaymentMethod>.DeleteAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(CardPaymentMethodRepository));
            throw;
        }
    }

    public async Task<IEnumerable<CardPaymentMethod>> FindAsync(Expression<Func<CardPaymentMethod, bool>> predicate)
    {
        try
        {
            var cards = _context.PaymentMethods.AsNoTracking()
                                               .OfType<CardPaymentMethod>()
                                               .Where(predicate)
                                               .AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully found card payment methods with predicate {Predicate}", nameof(CardPaymentMethodRepository), nameof(IGenericRepository<CardPaymentMethod>.FindAsync), predicate);
            return cards;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error finding card payment methods with predicate {Predicate}", nameof(CardPaymentMethodRepository), nameof(IGenericRepository<CardPaymentMethod>.FindAsync), predicate);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(CardPaymentMethodRepository));
            throw;
        }
    }

    public async Task<IEnumerable<CardPaymentMethod>> GetAllAsync()
    {
        try
        {
            var cards = _context.PaymentMethods.AsNoTracking()
                                               .OfType<CardPaymentMethod>()
                                               .AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved all card payment methods", nameof(CardPaymentMethodRepository), nameof(GetAllAsync));
            return cards;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving all card payment methods", nameof(CardPaymentMethodRepository), nameof(GetAllAsync));
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(CardPaymentMethodRepository));
            throw;
        }
    }

    public async Task<CardPaymentMethod?> GetByIdAsync(Guid id)
    {
        try
        {
            var card = await _context.PaymentMethods.AsNoTracking()
                                                    .OfType<CardPaymentMethod>()
                                                    .FirstOrDefaultAsync(c => c.Id == id);
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved card payment method by id {Id}", nameof(CardPaymentMethodRepository), nameof(GetByIdAsync), id);
            return card;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving card payment method by id {Id}", nameof(CardPaymentMethodRepository), nameof(GetByIdAsync), id);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(CardPaymentMethodRepository));
            throw;
        }
    }

    public async Task<CardPaymentMethod?> UpdateAsync(CardPaymentMethod entity)
    {
        try
        {
            _context.PaymentMethods.Update(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully updated card payment method {Entity}", nameof(CardPaymentMethodRepository), nameof(UpdateAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error updating card payment method {Entity}", nameof(CardPaymentMethodRepository), nameof(UpdateAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(CardPaymentMethodRepository));
            throw;
        }
    }
}



