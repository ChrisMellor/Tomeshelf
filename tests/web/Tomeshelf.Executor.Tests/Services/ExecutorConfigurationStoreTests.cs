using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Services;
using Tomeshelf.Executor.Tests.TestUtilities;

namespace Tomeshelf.Executor.Tests.Services;

public class ExecutorConfigurationStoreTests
{
    private const string SettingsEnvVar = "EXECUTOR_SETTINGS_DIR";

    [Fact]
    public async Task GetAsync_WhenMissingFiles_ReturnsDefaults()
    {
        var (directory, restore) = PrepareSettingsDirectory();
        try
        {
            var environment = new TestHostEnvironment
            {
                ContentRootPath = directory,
                EnvironmentName = "Development"
            };
            var store = new ExecutorConfigurationStore(environment, A.Fake<ILogger<ExecutorConfigurationStore>>());

            var options = await store.GetAsync();

            options.Enabled.Should().BeTrue();
            options.Endpoints.Should().BeEmpty();
        }
        finally
        {
            restore();
        }
    }

    [Fact]
    public async Task SaveAsync_WritesAndReadsBack()
    {
        var (directory, restore) = PrepareSettingsDirectory();
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
                        Headers = new Dictionary<string, string>
                        {
                            ["X-Test"] = "value"
                        }
                    }
                }
            };

            await store.SaveAsync(options);
            var loaded = await store.GetAsync();

            loaded.Enabled.Should().BeFalse();
            loaded.Endpoints.Should().ContainSingle();
            loaded.Endpoints[0].Name.Should().Be("Ping");
            loaded.Endpoints[0].Headers.Should().ContainKey("X-Test");
        }
        finally
        {
            restore();
        }
    }

    [Fact]
    public async Task GetAsync_WhenEnvironmentFileExists_UsesEnvironmentFile()
    {
        var (directory, restore) = PrepareSettingsDirectory();
        try
        {
            var environment = new TestHostEnvironment
            {
                ContentRootPath = directory,
                EnvironmentName = "Development"
            };

            var defaultPath = Path.Combine(directory, "executorSettings.json");
            var envPath = Path.Combine(directory, "executorSettings.Development.json");
            File.WriteAllText(defaultPath, SerializeOptions("Default"));
            File.WriteAllText(envPath, SerializeOptions("Environment"));

            var store = new ExecutorConfigurationStore(environment, A.Fake<ILogger<ExecutorConfigurationStore>>());
            var options = await store.GetAsync();

            options.Endpoints.Should().ContainSingle();
            options.Endpoints[0].Name.Should().Be("Environment");
        }
        finally
        {
            restore();
        }
    }

    private static string SerializeOptions(string name)
    {
        var payload = new
        {
            executor = new
            {
                enabled = true,
                endpoints = new[]
                {
                    new
                    {
                        name,
                        url = "https://example.test",
                        cron = "0 0 * * * ?",
                        enabled = true,
                        method = "POST"
                    }
                }
            }
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    private static (string Directory, Action Restore) PrepareSettingsDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "Tomeshelf.Executor.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var previous = Environment.GetEnvironmentVariable(SettingsEnvVar);
        Environment.SetEnvironmentVariable(SettingsEnvVar, null);

        return (directory, () =>
        {
            Environment.SetEnvironmentVariable(SettingsEnvVar, previous);
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        });
    }
}
