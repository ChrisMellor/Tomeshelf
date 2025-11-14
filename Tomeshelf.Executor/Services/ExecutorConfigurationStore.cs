using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tomeshelf.Executor.Configuration;

namespace Tomeshelf.Executor.Services;

public sealed class ExecutorConfigurationStore : IExecutorConfigurationStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _filePath;
    private readonly ILogger<ExecutorConfigurationStore> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public ExecutorConfigurationStore(IHostEnvironment environment, ILogger<ExecutorConfigurationStore> logger)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(logger);

        _filePath = Path.Combine(environment.ContentRootPath, "executorSettings.json");
        _logger = logger;
    }

    public async Task<ExecutorOptions> GetAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(_filePath))
            {
                _logger.LogInformation("Executor settings file '{Path}' not found. Returning defaults.", _filePath);
                return new ExecutorOptions();
            }

            await using var stream = File.OpenRead(_filePath);
            var document = await JsonSerializer.DeserializeAsync<ExecutorSettingsDocument>(stream, SerializerOptions, cancellationToken);
            return document?.Executor?.Clone() ?? new ExecutorOptions();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read executor settings file '{Path}'.", _filePath);
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
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

            await using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, document, SerializerOptions, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist executor settings to '{Path}'.", _filePath);
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    private sealed record ExecutorSettingsDocument
    {
        public ExecutorSettingsDocument()
        {
        }

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
            Endpoints = source.Endpoints.Select(Clone).ToList()
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
