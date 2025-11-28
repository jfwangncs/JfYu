using System;

namespace JfYu.Office
{
#if NETSTANDARD2_0
    /// <summary>
    /// Provides extension methods for handling argument null exceptions.
    /// </summary>
    public static class ArgumentNullExceptionExtension
    {

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
    }
#endif
}
