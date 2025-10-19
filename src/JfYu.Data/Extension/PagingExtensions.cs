using JfYu.Data.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JfYu.Data.Extension
{
    /// <summary>
    /// Paging extensions.
    /// </summary>
    public static class PagingExtensions
    {
        /// <summary>
        /// Converts an IQueryable into a paged result synchronously.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source.</typeparam>
        /// <param name="source">The IQueryable to be paginated.</param>
        /// <param name="pageIndex">The page index (1-based). Default is 1.</param>
        /// <param name="pageSize">The number of items per page. Default is 20.</param>
        /// <returns>A PagedData object containing the paginated data, total count, and total pages.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the source is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if pageIndex or pageSize is less than or equal to 0.</exception>
        public static PagedData<T> ToPaged<T>(this IQueryable<T> source, int pageIndex = 1, int pageSize = 20)
        {
            ArgumentNullException.ThrowIfNull(source);

            if (pageIndex <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageIndex), $"{nameof(pageIndex)} must be a positive integer greater than 0.");

            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), $"{nameof(pageSize)} must be a positive integer greater than 0.");

            int totalCount = source.Count();
            var list = source.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
            int totalPages = (int)Math.Ceiling((decimal)totalCount / pageSize);
            return new PagedData<T>() { TotalPages = totalPages, Data = list, TotalCount = totalCount };
        }

        /// <summary>
        /// Converts an IQueryable into a paged result with data transformation synchronously.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source.</typeparam>
        /// <typeparam name="Q">The type of elements after transformation.</typeparam>
        /// <param name="source">The IQueryable to be paginated.</param>
        /// <param name="func">A function to transform the paginated data.</param>
        /// <param name="pageIndex">The page index (1-based). Default is 1.</param>
        /// <param name="pageSize">The number of items per page. Default is 20.</param>
        /// <returns>A PagedData object containing the transformed paginated data, total count, and total pages.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the source or func is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if pageIndex or pageSize is less than or equal to 0.</exception>
        public static PagedData<Q> ToPaged<T, Q>(this IQueryable<T> source, Func<IEnumerable<T>, IEnumerable<Q>> func, int pageIndex = 1, int pageSize = 20)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(func);
            if (pageIndex <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageIndex), $"{nameof(pageIndex)} must be a positive integer greater than 0.");

            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), $"{nameof(pageSize)} must be a positive integer greater than 0.");

            int totalCount = source.Count();
            var list = source.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
            int totalPages = (int)Math.Ceiling((decimal)totalCount / pageSize);
            return new PagedData<Q>() { TotalPages = totalPages, Data = [.. func(list)], TotalCount = totalCount };
        }

        /// <summary>
        /// Converts an IQueryable into a paged result asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source.</typeparam>
        /// <param name="source">The IQueryable to be paginated.</param>
        /// <param name="pageIndex">The page index (1-based). Default is 1.</param>
        /// <param name="pageSize">The number of items per page. Default is 20.</param>
        /// <returns>A Task returning a PagedData object containing the paginated data, total count, and total pages.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the source is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if pageIndex or pageSize is less than or equal to 0.</exception>
        public static async Task<PagedData<T>> ToPagedAsync<T>(this IQueryable<T> source, int pageIndex = 1, int pageSize = 20)
        {
            ArgumentNullException.ThrowIfNull(source);

            if (pageIndex <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageIndex), $"{nameof(pageIndex)} must be a positive integer greater than 0.");

            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), $"{nameof(pageSize)} must be a positive integer greater than 0.");

            int totalCount = await source.CountAsync().ConfigureAwait(false);
            var list = await source.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToListAsync().ConfigureAwait(false);
            int totalPages = (int)Math.Ceiling((decimal)totalCount / pageSize);
            return new PagedData<T>() { TotalPages = totalPages, Data = list, TotalCount = totalCount };
        }

        /// <summary>
        /// Converts an IQueryable into a paged result with data transformation asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source.</typeparam>
        /// <typeparam name="Q">The type of elements after transformation.</typeparam>
        /// <param name="source">The IQueryable to be paginated.</param>
        /// <param name="func">A function to transform the paginated data.</param>
        /// <param name="pageIndex">The page index (1-based). Default is 1.</param>
        /// <param name="pageSize">The number of items per page. Default is 20.</param>
        /// <returns>A Task returning a PagedData object containing the transformed paginated data, total count, and total pages.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the source or func is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if pageIndex or pageSize is less than or equal to 0.</exception>
        public static async Task<PagedData<Q>> ToPagedAsync<T, Q>(this IQueryable<T> source, Func<IEnumerable<T>, IEnumerable<Q>> func, int pageIndex = 1, int pageSize = 20)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(func);
            if (pageIndex <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageIndex), $"{nameof(pageIndex)} must be a positive integer greater than 0.");

            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), $"{nameof(pageSize)} must be a positive integer greater than 0.");

            int totalCount = await source.CountAsync().ConfigureAwait(false);
            var list = await source.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToListAsync().ConfigureAwait(false);
            int totalPages = (int)Math.Ceiling((decimal)totalCount / pageSize);
            return new PagedData<Q>() { TotalPages = totalPages, Data = [.. func(list)], TotalCount = totalCount };
        }
    }
}