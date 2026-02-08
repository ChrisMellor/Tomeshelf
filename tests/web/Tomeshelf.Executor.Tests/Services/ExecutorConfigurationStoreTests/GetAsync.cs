using FakeItEasy;
using Microsoft.Extensions.Logging;
using Shouldly;
using Tomeshelf.Executor.Services;
using Tomeshelf.Executor.Tests.TestUtilities;

namespace Tomeshelf.Executor.Tests.Services.ExecutorConfigurationStoreTests;

public class GetAsync
{
    [Fact]
    public async Task WhenEnvironmentFileExists_UsesEnvironmentFile()
    {
        // Arrange
        // Act
        var (directory, restore) = ExecutorConfigurationStoreTestHarness.PrepareSettingsDirectory();
        // Assert
        try
        {
            var environment = new TestHostEnvironment
            {
                ContentRootPath = directory,
                EnvironmentName = "Development"
            };

            var defaultPath = Path.Combine(directory, "executorSettings.json");
            var envPath = Path.Combine(directory, "executorSettings.Development.json");
            File.WriteAllText(defaultPath, ExecutorConfigurationStoreTestHarness.SerializeOptions("Default"));
            File.WriteAllText(envPath, ExecutorConfigurationStoreTestHarness.SerializeOptions("Environment"));

            var store = new ExecutorConfigurationStore(environment, A.Fake<ILogger<ExecutorConfigurationStore>>());

            var options = await store.GetAsync();

            var endpoint = options.Endpoints.ShouldHaveSingleItem();
            endpoint.Name.ShouldBe("Environment");
        }
        finally
        {
            restore();
        }
    }

    [Fact]
    public async Task WhenMissingFiles_ReturnsDefaults()
    {
        // Arrange
        // Act
        var (directory, restore) = ExecutorConfigurationStoreTestHarness.PrepareSettingsDirectory();
        // Assert
        try
        {
            var environment = new TestHostEnvironment
            {
                ContentRootPath = directory,
                EnvironmentName = "Development"
            };
            var store = new ExecutorConfigurationStore(environment, A.Fake<ILogger<ExecutorConfigurationStore>>());

            var options = await store.GetAsync();

            options.Enabled.ShouldBeTrue();
            options.Endpoints.ShouldBeEmpty();
        }
        finally
        {
            restore();
        }
    }
}