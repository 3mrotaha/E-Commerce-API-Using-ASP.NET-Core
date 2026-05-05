using System.Linq.Expressions;
using eCommerce.Domain.Identity;
using eCommerce.Domain.Interfaces;
using eCommerce.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eCommerce.Persistence.Repositories;

public class SuperAdminRepository : ISuperAdminRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<SuperAdminRepository> _logger;

    public SuperAdminRepository(AppDbContext context, ILogger<SuperAdminRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SuperAdmin?> AddAsync(SuperAdmin entity)
    {
        try
        {
            _context.Accounts.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully added super admin {Entity}", nameof(SuperAdminRepository), nameof(AddAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error adding super admin {Entity}", nameof(SuperAdminRepository), nameof(AddAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(SuperAdminRepository));
            throw;
        }
    }

    public async Task DeleteAsync(SuperAdmin entity)
    {
        try
        {
            _context.Accounts.Remove(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully deleted super admin {Entity}", nameof(SuperAdminRepository), nameof(DeleteAsync), entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error deleting super admin {Entity}", nameof(SuperAdminRepository), nameof(DeleteAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(SuperAdminRepository));
            throw;
        }
    }

    public async Task<IEnumerable<SuperAdmin>> FindAsync(Expression<Func<SuperAdmin, bool>> predicate)
    {
        try
        {
            var superAdmins = _context.Accounts.AsNoTracking()
                                               .OfType<SuperAdmin>()
                                               .Where(predicate)
                                               .AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully found super admins with predicate {Predicate}", nameof(SuperAdminRepository), nameof(FindAsync), predicate);
            return superAdmins;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error finding super admins with predicate {Predicate}", nameof(SuperAdminRepository), nameof(FindAsync), predicate);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(SuperAdminRepository));
            throw;
        }
    }

    public async Task<IEnumerable<SuperAdmin>> GetAllAsync()
    {
        try
        {
            var superAdmins = _context.Accounts.AsNoTracking()
                                               .OfType<SuperAdmin>()
                                               .AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved all super admins", nameof(SuperAdminRepository), nameof(GetAllAsync));
            return superAdmins;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving all super admins", nameof(SuperAdminRepository), nameof(GetAllAsync));
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(SuperAdminRepository));
            throw;
        }
    }

    public async Task<SuperAdmin?> GetByIdAsync(Guid id)
    {
        try
        {
            var superAdmin = await _context.Accounts.AsNoTracking()
                                                    .OfType<SuperAdmin>()
                                                    .FirstOrDefaultAsync(a => a.Id == id);
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved super admin by id {Id}", nameof(SuperAdminRepository), nameof(GetByIdAsync), id);
            return superAdmin;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving super admin by id {Id}", nameof(SuperAdminRepository), nameof(GetByIdAsync), id);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(SuperAdminRepository));
            throw;
        }
    }

    public async Task<SuperAdmin?> UpdateAsync(SuperAdmin entity)
    {
        try
        {
            _context.Accounts.Update(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully updated super admin {Entity}", nameof(SuperAdminRepository), nameof(UpdateAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error updating super admin {Entity}", nameof(SuperAdminRepository), nameof(UpdateAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(SuperAdminRepository));
            throw;
        }
    }
}



