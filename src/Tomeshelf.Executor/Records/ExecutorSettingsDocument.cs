using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Extensions;

namespace Tomeshelf.Executor.Records;

public sealed record ExecutorSettingsDocument
{
    public ExecutorSettingsDocument() { }

    public ExecutorSettingsDocument(ExecutorOptions options)
    {
        Executor = options.Clone();
    }

    public ExecutorOptions Executor { get; set; } = new();
}