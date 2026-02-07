using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Controllers;
using Tomeshelf.Executor.Models;
using Tomeshelf.Executor.Services;
using Tomeshelf.Executor.Tests.TestUtilities;

namespace Tomeshelf.Executor.Tests.Controllers.HomeControllerTests;

public class Index
{
    [Fact]
    public async Task ReturnsSortedEndpointsAndApis()
    {
        // Arrange
        var options = new ExecutorOptions
        {
            Enabled = true,
            Endpoints = new List<EndpointScheduleOptions>
            {
                new EndpointScheduleOptions
                {
                    Name = "beta",
                    Url = "https://beta.test",
                    Cron = "0 0 * * * ?",
                    Method = "POST",
                    Enabled = true,
                    Headers = new Dictionary<string, string> { ["X-Test"] = "value" }
                },
                new EndpointScheduleOptions
                {
                    Name = "Alpha",
                    Url = "https://alpha.test",
                    Cron = "0 5 * * * ?",
                    Method = "GET",
                    Enabled = false
                }
            }
        };

        var apis = new List<ApiServiceDescriptor> { new ApiServiceDescriptor("mcm", "MCM", "https://mcm.test") };

        var controller = HomeControllerTestHarness.CreateController(options, apis, out _, out _, out _, out _);

        // Act
        var result = await controller.Index(CancellationToken.None);

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<ExecutorConfigurationViewModel>();
        model.Endpoints.Count.ShouldBe(2);
        model.Endpoints[0].Name.ShouldBe("Alpha");
        var api = model.ApiServices.ShouldHaveSingleItem();
        api.ServiceName.ShouldBe("mcm");
        model.Endpoints[1].HeadersDisplay.ShouldBe("X-Test:value");
    }
}
