using System;
using System.Collections.Generic;
using System.Linq;

namespace JfYu.Redis.Extensions
{
    /// <summary>
    /// Provides extension methods for handling argument null exceptions.
    /// </summary>
    public static class ArgumentNullExceptionExtension
    {
        /// <summary>
        /// Throws an exception if the specified collection is null, empty, or contains any null elements.
        /// </summary>
        /// <remarks>This method is intended for use in argument validation to ensure that a collection
        /// parameter is not only non-null, but also contains at least one non-null element. If any element in the
        /// collection is null, an ArgumentNullException is thrown for that element.</remarks>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="values">The collection to validate. Cannot be null, empty, or contain null elements.</param>
        /// <param name="paramName">The name of the parameter to include in any thrown exception. Defaults to "values".</param>
        /// <exception cref="ArgumentException">Thrown if the collection is empty.</exception>
        public static void ThrowIfNullOrEmpty<T>(this IEnumerable<T> values, string paramName = "values")
        {
            ArgumentNullException.ThrowIfNull(values);
            var list = values.ToList();
            if (list.Count == 0)
                throw new ArgumentException("Collection cannot be empty.", paramName);

            foreach (var value in list)
                ArgumentNullException.ThrowIfNull(value);
        }
    }
}