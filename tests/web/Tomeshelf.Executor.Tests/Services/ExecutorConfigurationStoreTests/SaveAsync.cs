using FakeItEasy;
using Microsoft.Extensions.Logging;
using Shouldly;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Services;
using Tomeshelf.Executor.Tests.TestUtilities;

namespace Tomeshelf.Executor.Tests.Services.ExecutorConfigurationStoreTests;

public class SaveAsync
{
    [Fact]
    public async Task WritesAndReadsBack()
    {
        // Arrange

        // Act

        // Assert

        var (directory, restore) = ExecutorConfigurationStoreTestHarness.PrepareSettingsDirectory();
        try
        {
            var environment = new TestHostEnvironment
            {
                ContentRootPath = directory,
                EnvironmentName = "Development"
            };
            var store = new ExecutorConfigurationStore(environment, A.Fake<ILogger<ExecutorConfigurationStore>>());

            var options = new ExecutorOptions
            {
                Enabled = false,
                Endpoints = new List<EndpointScheduleOptions>
                {
                    new()
                    {
                        Name = "Ping",
                        Url = "https://example.test",
                        Cron = "0 0 * * * ?",
                        Method = "PUT",
                        Enabled = true,
                        Headers = new Dictionary<string, string> { ["X-Test"] = "value" }
                    }
                }
            };

            await store.SaveAsync(options);
            var loaded = await store.GetAsync();

            loaded.Enabled.ShouldBeFalse();
            var endpoint = loaded.Endpoints.ShouldHaveSingleItem();
            endpoint.Name.ShouldBe("Ping");
            endpoint.Headers.ShouldNotBeNull();
            endpoint.Headers!.ContainsKey("X-Test")
                    .ShouldBeTrue();
        }
        finally
        {
            restore();
        }
    }
}