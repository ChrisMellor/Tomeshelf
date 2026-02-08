using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Tomeshelf.Executor.Tests.TestUtilities;

public sealed class TestHostEnvironment : IHostEnvironment
{
    public string ApplicationName { get; set; } = "Tomeshelf.Executor.Tests";

    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

    public string ContentRootPath { get; set; } = string.Empty;

    public string EnvironmentName { get; set; } = "Development";
}