using Microsoft.Extensions.DependencyInjection;
using Tomeshelf.Application.Shared;

namespace Tomeshelf.Application.Shared.Tests.ServiceCollectionExtensionsTests;

public class AddApplication
{
    [Fact]
    public void AddsServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplication();

        // Assert
        services.ShouldNotBeEmpty();
    }
}
