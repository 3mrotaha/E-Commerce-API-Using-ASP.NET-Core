using System.Linq.Expressions;
using eCommerce.Domain.Identity;
using eCommerce.Domain.Interfaces;
using eCommerce.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eCommerce.Persistence.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminRepository> _logger;

    public AdminRepository(AppDbContext context, ILogger<AdminRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Admin?> AddAsync(Admin entity)
    {
        try
        {
            _context.Accounts.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully added admin {Entity}", nameof(AdminRepository), nameof(AddAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error adding admin {Entity}", nameof(AdminRepository), nameof(AddAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(AdminRepository));
            throw;
        }
    }

    public async Task DeleteAsync(Admin entity)
    {
        try
        {
            _context.Accounts.Remove(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully deleted admin {Entity}", nameof(AdminRepository), nameof(DeleteAsync), entity);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error deleting admin {Entity}", nameof(AdminRepository), nameof(DeleteAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(AdminRepository));
            throw;
        }
    }

    public async Task<IEnumerable<Admin>> FindAsync(Expression<Func<Admin, bool>> predicate)
    {
        try
        {
            var admins = _context.Accounts.AsNoTracking()
                                        .OfType<Admin>()
                                        .Where(predicate)
                                        .AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully found admins with predicate {Predicate}", nameof(AdminRepository), nameof(FindAsync), predicate);
            return admins;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error finding admins with predicate {Predicate}", nameof(AdminRepository), nameof(FindAsync), predicate);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(AdminRepository));
            throw;
        }
    }

    public async Task<IEnumerable<Admin>> GetAllAsync()
    {
        try
        {
            var admins = _context.Accounts.AsNoTracking()
                                        .OfType<Admin>()
                                        .AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved all admins", nameof(AdminRepository), nameof(GetAllAsync));
            return admins;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving all admins", nameof(AdminRepository), nameof(GetAllAsync));
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(AdminRepository));
            throw;
        }
    }

    public async Task<Admin?> GetByIdAsync(Guid id)
    {
        try
        {
            var admin = await _context.Accounts.AsNoTracking()
                                        .OfType<Admin>()
                                        .FirstOrDefaultAsync(a => a.Id == id);
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved admin by id {Id}", nameof(AdminRepository), nameof(GetByIdAsync), id);
            return admin;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving admin by id {Id}", nameof(AdminRepository), nameof(GetByIdAsync), id);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(AdminRepository));
            throw;
        }
    }

    public async Task<Admin?> UpdateAsync(Admin entity)
    {
        try
        {
            _context.Accounts.Update(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully updated admin {Entity}", nameof(AdminRepository), nameof(UpdateAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error updating admin {Entity}", nameof(AdminRepository), nameof(UpdateAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(AdminRepository));
            throw;
        }
    }

}



