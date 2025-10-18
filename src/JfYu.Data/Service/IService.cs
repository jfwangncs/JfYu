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
    /// Service Interface.
    /// </summary>
    /// <typeparam name="T">The Type of entity Model.</typeparam>
    /// <typeparam name="TContext">The Type of Db Context.</typeparam>
    public interface IService<T, TContext> where T : BaseEntity
        where TContext : DbContext
    {
        /// <summary>
        /// Master context.
        /// </summary>
        public TContext Context { get; }

        /// <summary>
        /// Readonly context.
        /// </summary>
        public TContext ReadonlyContext { get; }

        /// <summary>
        /// Adds a single entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        /// <returns>The number of state entries written to the database.</returns>
        Task<int> AddAsync(T entity, CancellationToken cancellationToken= default);

        /// <summary>
        /// Adds a list of entities asynchronously.
        /// </summary>
        /// <param name="list">The list of entities to add.</param>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        /// <returns>The number of state entries written to the database.</returns>
        Task<int> AddAsync(List<T> list, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a single entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        /// <returns>The number of state entries written to the database.</returns>
        Task<int> UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a list of entities asynchronously.
        /// </summary>
        /// <param name="list">The list of entities to update.</param>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        /// <returns>The number of state entries written to the database.</returns>
        Task<int> UpdateAsync(List<T> list, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates entities matching the given predicate, applying a custom action on each.
        /// </summary>
        /// <param name="predicate">Condition to filter records.</param>
        /// <param name="selector">Action to apply on each record, with its index and entity.</param>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        /// <returns>The number of state entries written to the database.</returns>
        Task<int> UpdateAsync(Expression<Func<T, bool>> predicate, Action<int, T> selector, CancellationToken cancellationToken = default);

        /// <summary>
        /// Soft removes entities matching the given predicate.
        /// </summary>
        /// <param name="predicate">Condition to filter records.</param>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        /// <returns>The number of state entries written to the database.</returns>
        Task<int> RemoveAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Hard removes entities matching the given predicate.
        /// </summary>
        /// <param name="predicate">Condition to filter records.</param>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        /// <returns>The number of state entries written to the database.</returns>
        Task<int> HardRemoveAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single entity matching the given predicate asynchronously.
        /// </summary>
        /// <param name="predicate">Condition to filter records.</param>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        /// <returns>The entity matching the predicate, or null if no entity is found.</returns>
        Task<T?> GetOneAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a list of entities matching the given predicate asynchronously.
        /// </summary>
        /// <param name="predicate">Condition to filter records.</param>
        /// <param name="cancellationToken">CancellationToken for this operation.</param>
        /// <returns>A list of entities matching the predicate.</returns>
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