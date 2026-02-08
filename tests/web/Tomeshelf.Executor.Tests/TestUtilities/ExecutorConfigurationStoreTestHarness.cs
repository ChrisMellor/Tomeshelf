using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Tomeshelf.Executor.Tests.TestUtilities;

internal static class ExecutorConfigurationStoreTestHarness
{
    private const string SettingsEnvVar = "EXECUTOR_SETTINGS_DIR";

    public static (string Directory, Action Restore) PrepareSettingsDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "Tomeshelf.Executor.Tests", Guid.NewGuid()
                                                                                         .ToString("N"));
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

    public static string SerializeOptions(string name)
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
}
