using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tomeshelf.Executor.Configuration;

namespace Tomeshelf.Executor.Services;

public sealed class ExecutorConfigurationStore : IExecutorConfigurationStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly string _defaultFilePath;
    private readonly string? _environmentFilePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ILogger<ExecutorConfigurationStore> _logger;

    public ExecutorConfigurationStore(IHostEnvironment environment, ILogger<ExecutorConfigurationStore> logger)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(logger);

        _defaultFilePath = ExecutorSettingsPaths.GetDefaultFilePath(environment, true);
        _environmentFilePath = ExecutorSettingsPaths.GetEnvironmentFilePath(environment, true);

        _logger = logger;
    }

    public async Task<ExecutorOptions> GetAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var path = ResolveReadPath();
            if (!File.Exists(path))
            {
                _logger.LogInformation("Executor settings file '{Path}' not found. Returning defaults.", path);

                return new ExecutorOptions();
            }

            await using var stream = File.OpenRead(path);
            var document = await JsonSerializer.DeserializeAsync<ExecutorSettingsDocument>(stream, SerializerOptions, cancellationToken);

            return document?.Executor?.Clone() ?? new ExecutorOptions();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read executor settings file.");

            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveAsync(ExecutorOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var document = new ExecutorSettingsDocument(options);
            var path = ResolveWritePath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            _logger.LogInformation("Persisting executor settings to '{Path}'.", path);

            await using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, document, SerializerOptions, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist executor settings to disk.");

            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    private string ResolveReadPath()
    {
        if (!string.IsNullOrWhiteSpace(_environmentFilePath) && File.Exists(_environmentFilePath))
        {
            return _environmentFilePath;
        }

        return _defaultFilePath;
    }

    private string ResolveWritePath()
    {
        return _environmentFilePath ?? _defaultFilePath;
    }

    private sealed record ExecutorSettingsDocument
    {
        public ExecutorSettingsDocument() { }

        public ExecutorSettingsDocument(ExecutorOptions options)
        {
            Executor = options.Clone();
        }

        public ExecutorOptions Executor { get; set; } = new();
    }
}

internal static class ExecutorOptionsExtensions
{
    public static ExecutorOptions Clone(this ExecutorOptions source)
    {
        var clone = new ExecutorOptions
        {
            Enabled = source.Enabled,
            Endpoints = source.Endpoints.Select(Clone)
                                  .ToList()
        };

        return clone;
    }

    private static EndpointScheduleOptions Clone(EndpointScheduleOptions source)
    {
        return new EndpointScheduleOptions
        {
            Name = source.Name,
            Url = source.Url,
            Method = source.Method,
            Cron = source.Cron,
            Enabled = source.Enabled,
            TimeZone = source.TimeZone,
            Headers = source.Headers is null
                        ? null
                        : new Dictionary<string, string>(source.Headers, StringComparer.OrdinalIgnoreCase)
        };
    }
}