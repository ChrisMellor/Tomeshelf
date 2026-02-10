using System;
using System.Collections.Generic;
using System.Linq;

namespace Tomeshelf.Infrastructure.Shared;

internal static class EntityFrameworkExtensions
{
    /// <summary>
    ///     Removes the where.
    /// </summary>
    /// <typeparam name="T">The type of t.</typeparam>
    /// <param name="set">The set.</param>
    /// <param name="predicate">The predicate.</param>
    public static void RemoveWhere<T>(this ICollection<T> set, Func<T, bool> predicate)
    {
        foreach (var item in set.Where(predicate)
                                .ToList())
        {
            set.Remove(item);
        }
    }
}