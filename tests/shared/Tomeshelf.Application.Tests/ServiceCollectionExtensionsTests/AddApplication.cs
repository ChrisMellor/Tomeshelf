using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Tomeshelf.Application.Shared;

namespace Tomeshelf.Application.Tests.ServiceCollectionExtensionsTests;

public class AddApplication
{
    [Fact]
    public void ReturnsSameCollectionInstance()
    {
        var services = new ServiceCollection();

        var result = services.AddApplication();

        result.ShouldBeSameAs(services);
    }
}