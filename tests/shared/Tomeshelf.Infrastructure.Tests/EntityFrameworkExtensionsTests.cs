using System;
using System.Collections.Generic;
using System.Reflection;
using Tomeshelf.Infrastructure.Shared;
using Xunit;

namespace Tomeshelf.Infrastructure.Tests;

public class EntityFrameworkExtensionsTests
{
    [Fact]
    public void RemoveWhere_RemovesMatchingItems()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var method = GetRemoveWhereMethod(typeof(int));
        var predicate = new Func<int, bool>(value => value % 2 == 0);

        // Act
        method.Invoke(null, new object[] { items, predicate });

        // Assert
        Assert.Equal(new[] { 1, 3, 5 }, items);
    }

    [Fact]
    public void RemoveWhere_WhenNoMatches_LeavesCollectionUnchanged()
    {
        // Arrange
        var items = new List<string> { "alpha", "bravo" };
        var method = GetRemoveWhereMethod(typeof(string));
        var predicate = new Func<string, bool>(value => value == "charlie");

        // Act
        method.Invoke(null, new object[] { items, predicate });

        // Assert
        Assert.Equal(new[] { "alpha", "bravo" }, items);
    }

    private static MethodInfo GetRemoveWhereMethod(Type itemType)
    {
        var assembly = typeof(ServiceCollectionExtensions).Assembly;
        var type = assembly.GetType("Tomeshelf.Infrastructure.Shared.EntityFrameworkExtensions", throwOnError: true);
        var method = type!.GetMethod("RemoveWhere", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        Assert.NotNull(method);

        return method!.MakeGenericMethod(itemType);
    }
}
