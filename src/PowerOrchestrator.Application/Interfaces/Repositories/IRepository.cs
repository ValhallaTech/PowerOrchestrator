using PowerOrchestrator.Domain.Common;

namespace PowerOrchestrator.Application.Interfaces.Repositories;

/// <summary>
/// Generic repository interface for data access operations
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The entity if found, null otherwise</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all entities</returns>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities matching the specified criteria
    /// </summary>
    /// <param name="predicate">The search criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching entities</returns>
    Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity matching the specified criteria
    /// </summary>
    /// <param name="predicate">The search criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The first matching entity or null</returns>
    Task<T?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added entity</returns>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities
    /// </summary>
    /// <param name="entities">The entities to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <returns>The updated entity</returns>
    T Update(T entity);

    /// <summary>
    /// Updates multiple entities
    /// </summary>
    /// <param name="entities">The entities to update</param>
    void UpdateRange(IEnumerable<T> entities);

    /// <summary>
    /// Removes an entity
    /// </summary>
    /// <param name="entity">The entity to remove</param>
    void Remove(T entity);

    /// <summary>
    /// Removes an entity by ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if removed, false if not found</returns>
    Task<bool> RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes multiple entities
    /// </summary>
    /// <param name="entities">The entities to remove</param>
    void RemoveRange(IEnumerable<T> entities);

    /// <summary>
    /// Counts all entities
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total count of entities</returns>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching the specified criteria
    /// </summary>
    /// <param name="predicate">The search criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of matching entities</returns>
    Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity exists matching the criteria
    /// </summary>
    /// <param name="predicate">The search criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if any entity matches, false otherwise</returns>
    Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}
