using Microsoft.Extensions.DependencyInjection;
using Tomeshelf.Application.Shared;
using Xunit;

namespace Tomeshelf.Application.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddApplication_ReturnsSameCollectionInstance()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddApplication();

        // Assert
        Assert.Same(services, result);
    }
}
