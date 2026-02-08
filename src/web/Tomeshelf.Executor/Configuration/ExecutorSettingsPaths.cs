using System;
using System.IO;
using Microsoft.Extensions.Hosting;

namespace Tomeshelf.Executor.Configuration;

internal static class ExecutorSettingsPaths
{
    public const string SettingsDirectoryEnvironmentVariable = "EXECUTOR_SETTINGS_DIR";
    private const string DefaultSettingsFileName = "executorSettings.json";

    public static void EnsureSeedFiles(IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);

        var sourceDefault = Path.Combine(environment.ContentRootPath, DefaultSettingsFileName);
        var targetDefault = GetDefaultFilePath(environment, true);
        CopyIfMissing(sourceDefault, targetDefault);

        if (string.IsNullOrWhiteSpace(environment.EnvironmentName))
        {
            return;
        }

        var sourceEnvironment = Path.Combine(environment.ContentRootPath, $"executorSettings.{environment.EnvironmentName}.json");
        var targetEnvironment = GetEnvironmentFilePath(environment, true);
        if (targetEnvironment is not null)
        {
            CopyIfMissing(sourceEnvironment, targetEnvironment);
        }
    }

    public static string GetDefaultFilePath(IHostEnvironment environment, bool ensureDirectory = false)
    {
        return Path.Combine(GetDirectory(environment, ensureDirectory), DefaultSettingsFileName);
    }

    public static string GetDirectory(IHostEnvironment environment, bool ensureExists = false)
    {
        ArgumentNullException.ThrowIfNull(environment);

        var directory = Environment.GetEnvironmentVariable(SettingsDirectoryEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(directory))
        {
            directory = environment.ContentRootPath;
        }
        else if (!Path.IsPathRooted(directory))
        {
            directory = Path.Combine(environment.ContentRootPath, directory);
        }

        if (ensureExists)
        {
            Directory.CreateDirectory(directory);
        }

        return directory;
    }

    public static string? GetEnvironmentFilePath(IHostEnvironment environment, bool ensureDirectory = false)
    {
        if (string.IsNullOrWhiteSpace(environment.EnvironmentName))
        {
            return null;
        }

        return Path.Combine(GetDirectory(environment, ensureDirectory), $"executorSettings.{environment.EnvironmentName}.json");
    }

    private static void CopyIfMissing(string sourcePath, string targetPath)
    {
        if (!File.Exists(sourcePath) || File.Exists(targetPath))
        {
            return;
        }

        var targetDirectory = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrEmpty(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        File.Copy(sourcePath, targetPath);
    }
}