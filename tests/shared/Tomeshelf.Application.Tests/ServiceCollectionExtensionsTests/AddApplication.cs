using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Tomeshelf.Application.Shared;

namespace Tomeshelf.Application.Tests.ServiceCollectionExtensionsTests;

public class AddApplication
{
    /// <summary>
    ///     Returns the same collection instance.
    /// </summary>
    [Fact]
    public void ReturnsSameCollectionInstance()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddApplication();

        // Assert
        result.ShouldBeSameAs(services);
    }
}