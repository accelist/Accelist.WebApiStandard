using System.Diagnostics.CodeAnalysis;

namespace System
{
    public static class StringHasValueExtensions
    {
        /// <summary>
        /// Indicates whether the specified string is not <see langword="null"/> nor an empty string ("").
        /// </summary>
        /// <param name="value"></param>
        /// <returns><see langword="false"/> if the value parameter is <see langword="null"/> or an empty string (""); otherwise, <see langword="true"/>.</returns>
        public static bool HasValue([NotNullWhen(true)] this string? value)
        {
            return !string.IsNullOrEmpty(value);
        }
    }
}
