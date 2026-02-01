using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Tomeshelf.Application.Shared;

namespace Tomeshelf.Application.Tests.ServiceCollectionExtensionsTests;

public class AddApplication
{
    [Fact]
    public void ReturnsSameCollectionInstance()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddApplication();

        // Assert
        result.Should().BeSameAs(services);
    }
}
