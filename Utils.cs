using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MongoDB.AspNet.Identity
{
    internal static class Utils
    {
        internal static IList<T> ToIList<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.ToList();
        }
    }
}