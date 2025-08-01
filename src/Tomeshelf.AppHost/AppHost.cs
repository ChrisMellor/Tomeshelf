using Aspire.Hosting;

namespace Tomeshelf.AppHost;

internal class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        var apiService = builder.AddProject<Projects.Tomeshelf_Api>("Api")
            .WithHttpHealthCheck("/health")
            .WithUrl("/swagger");

        builder.AddProject<Projects.Tomeshelf_Web>("Web")
            .WithExternalHttpEndpoints()
            .WithHttpHealthCheck("/health")
            .WithReference(apiService)
            .WaitFor(apiService);

        builder.Build().Run();
    }
}