using JfYu.Data.Context;
using JfYu.Data.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace JfYu.Data.Service
{
    /// <summary>
    /// Services
    /// </summary>
    /// <typeparam name="T">Entity Model</typeparam>
    /// <typeparam name="TContext">Db Context</typeparam>
    public class Service<T, TContext>(TContext context, ReadonlyDBContext<TContext> readonlyDBContext) : IService<T, TContext> where T : BaseEntity
            where TContext : DbContext
    {
        /// <inheritdoc/>
        public TContext Context { get; } = context;

        /// <inheritdoc/>
        public TContext ReadonlyContext { get; } = readonlyDBContext.Current;

        /// <inheritdoc/>
        public virtual async Task<int> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            entity.CreatedTime = entity.UpdatedTime = DateTime.UtcNow;
            await Context.AddAsync(entity,cancellationToken).ConfigureAwait(false);
            return await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<int> AddAsync(List<T> list, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            for (int i = 0; i < list.Count; i++)
            {
                list[i].CreatedTime = list[i].UpdatedTime = now;
            }
            await Context.AddRangeAsync(list, cancellationToken).ConfigureAwait(false);
            return await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<int> UpdateAsync(T entity , CancellationToken cancellationToken = default)
        {
            entity.UpdatedTime = DateTime.UtcNow;
            Context.Update(entity);
            int saveChanges = await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return saveChanges;
        }

        /// <inheritdoc/>
        public virtual async Task<int> UpdateAsync(List<T> list, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            for (int i = 0; i < list.Count; i++)
            {
                list[i].UpdatedTime = now;
            }
            Context.UpdateRange(list);
            int saveChanges = await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return saveChanges;
        }

        /// <inheritdoc/>
        public virtual async Task<int> UpdateAsync(Expression<Func<T, bool>> predicate, Action<int, T> selector, CancellationToken cancellationToken = default)
        {
            if (predicate == null || selector == null)
                return 0;
            var data = await Context.Set<T>().Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);
            var now = DateTime.UtcNow;
            for (int i = 0; i < data.Count; i++)
            {
                selector(i, data[i]);
                data[i].UpdatedTime = now;
            }

            return await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<int> RemoveAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
                return 0;
            var list = await GetListAsync(predicate, cancellationToken).ConfigureAwait(false);
            var now = DateTime.UtcNow;
            for (int i = 0; i < list.Count; i++)
            {
                list[i].UpdatedTime = now;
                list[i].Status = (int)DataStatus.Disable;
                Context.Update(list[i]);
            }
            return await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<int> HardRemoveAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
                return 0;

            var lists = await GetListAsync(predicate, cancellationToken).ConfigureAwait(false);
            for (int i = 0; i < lists.Count; i++)
            {
                Context.Remove(lists[i]);
            }
            return await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<T?> GetOneAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            return predicate switch
            {
                null => await ReadonlyContext.Set<T>().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false),
                _ => await ReadonlyContext.Set<T>().FirstOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false)
            };
        }

        /// <inheritdoc/>
        public virtual async Task<IList<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            return predicate switch
            {
                null => await ReadonlyContext.Set<T>().ToListAsync(cancellationToken).ConfigureAwait(false),
                _ => await ReadonlyContext.Set<T>().Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false)
            };
        }

        /// <inheritdoc/>
        public virtual async Task<IList<T1>> GetSelectListAsync<T1>(Expression<Func<T, T1>> selector, Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            if (selector == null)
                return [];
            return predicate switch
            {
                null => await ReadonlyContext.Set<T>().Select(selector).ToListAsync(cancellationToken).ConfigureAwait(false),
                _ => await ReadonlyContext.Set<T>().Where(predicate).Select(selector).ToListAsync(cancellationToken).ConfigureAwait(false)
            };
        }
    }
}