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
#if NETSTANDARD2_0
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the argument is null.
        /// </summary>
        /// <param name="argument">The argument to validate.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException">Thrown if the argument is null.</exception>
        public static void ThrowIfNull(object? argument, string? paramName = null)
        {
            if (argument is null)
                throw new ArgumentNullException(paramName);
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the argument is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="argument">The string argument to validate.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException">Thrown if the argument is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the argument is empty or whitespace.</exception>
        public static void ThrowIfNullOrWhiteSpace(string? argument, string? paramName = null)
        {
            if (argument is null)
                throw new ArgumentNullException(paramName);

            if (string.IsNullOrWhiteSpace(argument))
                throw new ArgumentException("The value cannot be an empty string or composed entirely of whitespace.", paramName);
        }
#endif

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
#if NETSTANDARD2_0
            ThrowIfNull(values, paramName);
#else
            ArgumentNullException.ThrowIfNull(values);
#endif
            var list = values.ToList();
            if (list.Count == 0)
                throw new ArgumentException("Collection cannot be empty.", paramName);

            foreach (var value in list)
#if NETSTANDARD2_0
                ThrowIfNull(value, paramName);
#else
                ArgumentNullException.ThrowIfNull(value);
#endif
        }
    }
}