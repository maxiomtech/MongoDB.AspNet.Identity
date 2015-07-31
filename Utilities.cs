namespace MongoDB.AspNet.Identity
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The utilities class.
    /// </summary>
    internal static class Utilities
    {
        #region Methods

        /// <summary>
        ///     Converts an IEnumberable of T to a IList of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns>IList{T}.</returns>
        internal static IList<T> ToIList<T>(this IEnumerable<T> enumerable) => enumerable.ToList();

        #endregion
    }
}
