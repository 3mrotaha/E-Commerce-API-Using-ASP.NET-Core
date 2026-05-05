using System.Linq.Expressions;
using eCommerce.Domain.Identity;
using eCommerce.Domain.Interfaces;
using eCommerce.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eCommerce.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> AddAsync(User entity)
    {
        try
        {
            _context.Accounts.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully added user {Entity}", nameof(UserRepository), nameof(AddAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error adding user {Entity}", nameof(UserRepository), nameof(AddAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(UserRepository));
            throw;
        }
    }

    public async Task DeleteAsync(User entity)
    {
        try
        {
            _context.Accounts.Remove(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully deleted user {Entity}", nameof(UserRepository), nameof(DeleteAsync), entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error deleting user {Entity}", nameof(UserRepository), nameof(DeleteAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(UserRepository));
            throw;
        }
    }

    public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate)
    {
        try
        {
            var users = _context.Accounts.AsNoTracking()
                                         .OfType<User>()
                                         .Where(predicate)
                                         .AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully found users with predicate {Predicate}", nameof(UserRepository), nameof(FindAsync), predicate);
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error finding users with predicate {Predicate}", nameof(UserRepository), nameof(FindAsync), predicate);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(UserRepository));
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            var users = _context.Accounts.AsNoTracking()
                                         .OfType<User>()
                                         .AsEnumerable();
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved all users", nameof(UserRepository), nameof(GetAllAsync));
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving all users", nameof(UserRepository), nameof(GetAllAsync));
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(UserRepository));
            throw;
        }
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        try
        {
            var user = await _context.Accounts.AsNoTracking()
                                              .OfType<User>()
                                              .FirstOrDefaultAsync(u => u.Id == id);
            _logger.LogInformation("{Repository}-{Method} - Successfully retrieved user by id {Id}", nameof(UserRepository), nameof(GetByIdAsync), id);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error retrieving user by id {Id}", nameof(UserRepository), nameof(GetByIdAsync), id);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(UserRepository));
            throw;
        }
    }

    public async Task<User?> UpdateAsync(User entity)
    {
        try
        {
            _context.Accounts.Update(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("{Repository}-{Method} - Successfully updated user {Entity}", nameof(UserRepository), nameof(UpdateAsync), entity);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Repository}-{Method} - Error updating user {Entity}", nameof(UserRepository), nameof(UpdateAsync), entity);
            _logger.LogCritical(ex, "{Repository} - Critical repository failure", nameof(UserRepository));
            throw;
        }
    }
}



