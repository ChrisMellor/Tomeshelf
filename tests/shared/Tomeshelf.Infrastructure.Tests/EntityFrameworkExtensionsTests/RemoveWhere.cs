using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Tomeshelf.Infrastructure.Shared;

namespace Tomeshelf.Infrastructure.Tests.EntityFrameworkExtensionsTests;

public class RemoveWhere
{
    [Fact]
    public void RemovesMatchingItems()
    {
        // Arrange
        var items = new List<int>
        {
            1,
            2,
            3,
            4,
            5
        };
        var method = GetRemoveWhereMethod(typeof(int));
        var predicate = new Func<int, bool>(value => (value % 2) == 0);

        // Act
        method.Invoke(null, new object[] { items, predicate });

        // Assert
        items.Should()
             .Equal(1, 3, 5);
    }

    [Fact]
    public void WhenNoMatches_LeavesCollectionUnchanged()
    {
        // Arrange
        var items = new List<string>
        {
            "alpha",
            "bravo"
        };
        var method = GetRemoveWhereMethod(typeof(string));
        var predicate = new Func<string, bool>(value => value == "charlie");

        // Act
        method.Invoke(null, new object[] { items, predicate });

        // Assert
        items.Should()
             .Equal("alpha", "bravo");
    }

    private static MethodInfo GetRemoveWhereMethod(Type itemType)
    {
        var assembly = typeof(ServiceCollectionExtensions).Assembly;
        var type = assembly.GetType("Tomeshelf.Infrastructure.Shared.EntityFrameworkExtensions", true);
        var method = type!.GetMethod("RemoveWhere", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        method.Should()
              .NotBeNull();

        return method!.MakeGenericMethod(itemType);
    }
}