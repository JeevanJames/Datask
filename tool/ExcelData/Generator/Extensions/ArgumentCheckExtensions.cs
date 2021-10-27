using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Datask.Tool.ExcelData.Generator.Extensions
{
    public static class ArgumentCheckExtensions
    {
        /// <summary>
        ///     Asserts that the specified string parameter is not null or empty.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <param name="argName">
        ///     The name of the string parameter. Use <c>nameof</c> to get the name in C#.
        /// </param>
        /// <param name="emptyErrorMessage">
        ///     The error message for the <see cref="ArgumentException"/> that is thrown if the
        ///     <paramref name="str"/> is empty.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if <paramref name="str"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown if <paramref name="str"/> is empty. The message used for this exception comes
        ///     from the <paramref name="emptyErrorMessage"/> parameter.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AssertNotNullOrEmpty(this string? str, string argName,
            [Localizable(false)] string emptyErrorMessage)
        {
            if (str is null)
                throw new ArgumentNullException(argName);
            if (str.Length == 0)
                throw new ArgumentException(emptyErrorMessage ?? string.Empty, argName);
        }

        /// <summary>
        ///     Asserts that the specified string parameter is not null, empty or whitespace.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <param name="argName">
        ///     The name of the string parameter. Use <c>nameof</c> to get the name in C#.
        /// </param>
        /// <param name="whitespaceErrorMessage">
        ///     The error message for the <see cref="ArgumentException"/> that us thrown if the
        ///     <paramref name="str"/> parameter is whitespace.
        /// </param>
        /// <param name="emptyErrorMessage">
        ///     The error message for the <see cref="ArgumentException"/> that is thrown if the
        ///     <paramref name="str"/> parameter is empty. If not specified,
        ///     the <paramref name="whitespaceErrorMessage"/> is used.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if <paramref name="str"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="str"/> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AssertNotNullOrWhitespace(this string? str, string argName,
            [Localizable(false)] string whitespaceErrorMessage,
            [Localizable(false)] string? emptyErrorMessage = null)
        {
            if (str is null)
                throw new ArgumentNullException(argName);
            if (str.Length == 0)
                throw new ArgumentException(emptyErrorMessage ?? whitespaceErrorMessage, argName);
            if (str.Trim().Length == 0)
                throw new ArgumentException(whitespaceErrorMessage, argName);
        }
    }
}
