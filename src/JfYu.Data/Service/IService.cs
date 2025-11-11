using JfYu.Data.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace JfYu.Data.Service
{
    /// <summary>
    /// Generic service interface providing CRUD operations with automatic read-write separation.
    /// Implements common data access patterns for entities inheriting from BaseEntity.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
    /// <typeparam name="TContext">The DbContext type for database operations.</typeparam>
    /// <remarks>
    /// This service automatically routes:
    /// - Write operations (Add, Update, Remove) to the master Context
    /// - Read operations (GetOne, GetList, GetSelectList) to the ReadonlyContext (randomly selected slave or master if no slaves configured)
    /// </remarks>
    public interface IService<T, TContext> where T : BaseEntity
        where TContext : DbContext
    {
        /// <summary>
        /// Gets the master database context for write operations.
        /// All Add, Update, and Remove operations use this context.
        /// </summary>
        public TContext Context { get; }

        /// <summary>
        /// Gets the readonly database context for read operations.
        /// Randomly selects from configured read replicas or falls back to master if none configured.
        /// All query operations use this context for load balancing.
        /// </summary>
        public TContext ReadonlyContext { get; }

        /// <summary>
        /// Adds a single entity asynchronously to the master database.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>The number of state entries written to the database (typically 1 on success).</returns>
        Task<int> AddAsync(T entity, CancellationToken cancellationToken= default);

        /// <summary>
        /// Adds multiple entities asynchronously to the master database in a single transaction.
        /// </summary>
        /// <param name="list">The list of entities to add.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>The number of state entries written to the database (equals list.Count on success).</returns>
        Task<int> AddAsync(List<T> list, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a single entity asynchronously in the master database.
        /// The entity must already be tracked or will be attached before updating.
        /// </summary>
        /// <param name="entity">The entity to update with modified properties.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>The number of state entries written to the database (typically 1 on success).</returns>
        Task<int> UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates multiple entities asynchronously in the master database in a single transaction.
        /// </summary>
        /// <param name="list">The list of entities to update.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>The number of state entries written to the database (equals list.Count on success).</returns>
        Task<int> UpdateAsync(List<T> list, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates entities matching the predicate by applying a custom action to each entity.
        /// Useful for bulk updates with custom logic based on index or entity properties.
        /// </summary>
        /// <param name="predicate">Expression to filter which entities to update.</param>
        /// <param name="selector">Action to apply to each entity, receiving its index and the entity instance.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>The number of state entries written to the database.</returns>
        /// <example>
        /// <code>
        /// await service.UpdateAsync(u => u.IsActive, (index, user) => user.UserName = $"User{index}");
        /// </code>
        /// </example>
        Task<int> UpdateAsync(Expression<Func<T, bool>> predicate, Action<int, T> selector, CancellationToken cancellationToken = default);

        /// <summary>
        /// Soft deletes entities matching the predicate by setting their Status to Disable.
        /// The entities remain in the database but are marked as inactive.
        /// </summary>
        /// <param name="predicate">Expression to filter which entities to soft delete.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>The number of state entries written to the database.</returns>
        Task<int> RemoveAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Permanently deletes entities matching the predicate from the database.
        /// This operation cannot be undone. Use RemoveAsync for soft deletion instead.
        /// </summary>
        /// <param name="predicate">Expression to filter which entities to permanently delete.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>The number of state entries written to the database.</returns>
        Task<int> HardRemoveAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a single entity from the readonly context matching the predicate.
        /// Returns null if no matching entity is found.
        /// </summary>
        /// <param name="predicate">Optional expression to filter the entity. If null, returns the first entity.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>The first matching entity, or null if not found.</returns>
        Task<T?> GetOneAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of entities from the readonly context matching the predicate.
        /// </summary>
        /// <param name="predicate">Optional expression to filter entities. If null, returns all entities.</param>
        /// <param name="cancellationToken">Cancellation token for the async operation.</param>
        /// <returns>A list of matching entities (may be empty if no matches found).</returns>
        Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a list of entities matching the given predicate and projects them to a different type in the database.
        /// </summary>
        /// <typeparam name="T1">The type of the result after projection.</typeparam>
        /// <param name="selector">A function to project the entity to a different type.</param>
        /// <param name="predicate">Condition to filter records.</param>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        /// <returns>A list of projected entities.</returns>
        Task<IList<T1>> GetSelectListAsync<T1>(Expression<Func<T, T1>> selector, Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    }
}