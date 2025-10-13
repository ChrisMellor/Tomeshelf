using System;
using System.Collections.Generic;
using System.Linq;

namespace Tomeshelf.Infrastructure;

internal static class EntityFrameworkExtensions
{
    public static void RemoveWhere<T>(this ICollection<T> set, Func<T, bool> predicate)
    {
        foreach (var item in set.Where(predicate).ToList()) set.Remove(item);
    }
}