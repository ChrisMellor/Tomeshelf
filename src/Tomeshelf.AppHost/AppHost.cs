using Aspire.Hosting;

namespace Tomeshelf.AppHost;

internal class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        // API
        var api = builder.AddProject<Projects.Tomeshelf_Api>("Api")
            .WithHttpHealthCheck("/health");

        // Web
        builder.AddProject<Projects.Tomeshelf_Web>("Web")
            .WithExternalHttpEndpoints()
            .WithHttpHealthCheck("/health")
            .WithReference(api)
            .WaitFor(api);

        builder.Build().Run();
    }
}