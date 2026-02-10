using System;
using System.IO;
using Microsoft.Extensions.Hosting;

namespace Tomeshelf.Executor.Configuration;

internal static class ExecutorSettingsPaths
{
    public const string SettingsDirectoryEnvironmentVariable = "EXECUTOR_SETTINGS_DIR";
    private const string DefaultSettingsFileName = "executorSettings.json";

    /// <summary>
    ///     Ensures the seed files.
    /// </summary>
    /// <param name="environment">The environment.</param>
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

    /// <summary>
    ///     Gets the default file path.
    /// </summary>
    /// <param name="environment">The environment.</param>
    /// <param name="ensureDirectory">The ensure directory.</param>
    /// <returns>The resulting string.</returns>
    public static string GetDefaultFilePath(IHostEnvironment environment, bool ensureDirectory = false)
    {
        return Path.Combine(GetDirectory(environment, ensureDirectory), DefaultSettingsFileName);
    }

    /// <summary>
    ///     Gets the directory.
    /// </summary>
    /// <param name="environment">The environment.</param>
    /// <param name="ensureExists">The ensure exists.</param>
    /// <returns>The resulting string.</returns>
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

    /// <summary>
    ///     Gets the environment file path.
    /// </summary>
    /// <param name="environment">The environment.</param>
    /// <param name="ensureDirectory">The ensure directory.</param>
    /// <returns>The result of the operation.</returns>
    public static string? GetEnvironmentFilePath(IHostEnvironment environment, bool ensureDirectory = false)
    {
        if (string.IsNullOrWhiteSpace(environment.EnvironmentName))
        {
            return null;
        }

        return Path.Combine(GetDirectory(environment, ensureDirectory), $"executorSettings.{environment.EnvironmentName}.json");
    }

    /// <summary>
    ///     Copys the if missing.
    /// </summary>
    /// <param name="sourcePath">The source path.</param>
    /// <param name="targetPath">The target path.</param>
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