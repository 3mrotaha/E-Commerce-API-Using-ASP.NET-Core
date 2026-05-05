using System.Linq.Expressions;

namespace eCommerce.Domain;

public interface IGenericRepository<T> 
where T : class
{
    /// <summary>
    /// Adds a new entity to the repository asynchronously.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>The added entity if successful; otherwise, null.</returns>
    Task<T?> AddAsync(T entity);

    /// <summary>
    /// Updates an existing entity in the repository asynchronously.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>The updated entity if successful; otherwise, null.</returns>
    Task<T?> UpdateAsync(T entity);

    /// <summary>
    /// Deletes an entity from the repository asynchronously.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(T entity);

    /// <summary>
    /// Gets an entity by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all entities from the repository asynchronously.
    /// </summary>
    /// <returns>A collection of all entities.</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Finds entities in the repository that match the specified predicate asynchronously.
    /// </summary>
    /// <param name="predicate">The predicate to filter entities.</param>
    /// <returns>A collection of entities that match the predicate.</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
}
