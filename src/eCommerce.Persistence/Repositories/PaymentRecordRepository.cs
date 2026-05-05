using System.Linq.Expressions;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using eCommerce.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eCommerce.Persistence.Repositories;

public class PaymentRecordRepository : IPaymentRecordRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<PaymentRecordRepository> _logger;

    public PaymentRecordRepository(AppDbContext context, ILogger<PaymentRecordRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaymentRecord?> AddAsync(PaymentRecord entity)
    {
        try
        {
            _context.PaymentRecords.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully added payment record {Entity}", nameof(PaymentRecordRepository), nameof(AddAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error adding payment record {Entity}", nameof(PaymentRecordRepository), nameof(AddAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(PaymentRecordRepository));
            throw;
        }
    }

    public async Task DeleteAsync(PaymentRecord entity)
    {
        try
        {
            _context.PaymentRecords.Remove(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully deleted payment record {Entity}", nameof(PaymentRecordRepository), nameof(DeleteAsync), entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error deleting payment record {Entity}", nameof(PaymentRecordRepository), nameof(DeleteAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(PaymentRecordRepository));
            throw;
        }
    }

    public async Task<IEnumerable<PaymentRecord>> FindAsync(Expression<Func<PaymentRecord, bool>> predicate)
    {
        try
        {
            var paymentRecords = _context.PaymentRecords.AsNoTracking()
                                                        .Where(predicate)
                                                        .AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully found payment records with predicate {Predicate}", nameof(PaymentRecordRepository), nameof(FindAsync), predicate);
            return paymentRecords;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error finding payment records with predicate {Predicate}", nameof(PaymentRecordRepository), nameof(FindAsync), predicate);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(PaymentRecordRepository));
            throw;
        }
    }

    public async Task<IEnumerable<PaymentRecord>> GetAllAsync()
    {
        try
        {
            var paymentRecords = _context.PaymentRecords.AsNoTracking().AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved all payment records", nameof(PaymentRecordRepository), nameof(GetAllAsync));
            return paymentRecords;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving all payment records", nameof(PaymentRecordRepository), nameof(GetAllAsync));
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(PaymentRecordRepository));
            throw;
        }
    }

    public async Task<PaymentRecord?> GetByIdAsync(Guid id)
    {
        try
        {
            var paymentRecord = await _context.PaymentRecords.AsNoTracking().FirstOrDefaultAsync(pr => pr.Id == id);
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved payment record by id {Id}", nameof(PaymentRecordRepository), nameof(GetByIdAsync), id);
            return paymentRecord;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving payment record by id {Id}", nameof(PaymentRecordRepository), nameof(GetByIdAsync), id);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(PaymentRecordRepository));
            throw;
        }
    }

    public async Task<PaymentRecord?> UpdateAsync(PaymentRecord entity)
    {
        try
        {
            _context.PaymentRecords.Update(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully updated payment record {Entity}", nameof(PaymentRecordRepository), nameof(UpdateAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error updating payment record {Entity}", nameof(PaymentRecordRepository), nameof(UpdateAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(PaymentRecordRepository));
            throw;
        }
    }
}



