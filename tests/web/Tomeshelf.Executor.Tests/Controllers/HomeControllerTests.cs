using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Tomeshelf.Executor.Configuration;
using Tomeshelf.Executor.Controllers;
using Tomeshelf.Executor.Models;
using Tomeshelf.Executor.Services;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.Executor.Tests.Controllers;

public class HomeControllerTests
{
    [Fact]
    public async Task Create_Get_ReturnsEditorDefaults()
    {
        var controller = CreateController(new ExecutorOptions(), new List<ApiServiceDescriptor>(), out _, out _, out _, out _);

        var result = await controller.Create(CancellationToken.None);

        var view = result.Should()
                         .BeOfType<ViewResult>()
                         .Subject;
        var model = view.Model
                        .Should()
                        .BeOfType<ExecutorConfigurationViewModel>()
                        .Subject;
        model.Editor
             .Enabled
             .Should()
             .BeTrue();
        model.Editor
             .Method
             .Should()
             .Be("POST");
    }

    [Fact]
    public async Task Create_Post_DuplicateName_ReturnsViewWithError()
    {
        var options = new ExecutorOptions
        {
            Endpoints = new List<EndpointScheduleOptions>
            {
                new EndpointScheduleOptions
                {
                    Name = "Existing",
                    Url = "https://example.test",
                    Cron = "0 0 * * * ?"
                }
            }
        };
        var controller = CreateController(options, new List<ApiServiceDescriptor>(), out var store, out _, out _, out _);

        var model = new EndpointEditorModel
        {
            Name = "existing",
            Url = "https://new.test",
            Cron = "0 0 * * * ?",
            Method = "POST"
        };

        var result = await controller.Create(model, CancellationToken.None);

        result.Should()
              .BeOfType<ViewResult>()
              .Which
              .ViewName
              .Should()
              .Be("Create");
        controller.ModelState[nameof(EndpointEditorModel.Name)]!.Errors
                  .Should()
                  .NotBeEmpty();
        A.CallTo(() => store.SaveAsync(A<ExecutorOptions>._, A<CancellationToken>._))
         .MustNotHaveHappened();
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsView()
    {
        var options = new ExecutorOptions();
        var controller = CreateController(options, new List<ApiServiceDescriptor>(), out var store, out _, out _, out _);
        controller.ModelState.AddModelError("Name", "Required");

        var model = new EndpointEditorModel { Name = "Endpoint" };
        var result = await controller.Create(model, CancellationToken.None);

        result.Should()
              .BeOfType<ViewResult>()
              .Which
              .ViewName
              .Should()
              .Be("Create");
        A.CallTo(() => store.SaveAsync(A<ExecutorOptions>._, A<CancellationToken>._))
         .MustNotHaveHappened();
    }

    [Fact]
    public async Task Create_Post_Valid_AddsEndpointAndRedirects()
    {
        var options = new ExecutorOptions();
        var controller = CreateController(options, new List<ApiServiceDescriptor>(), out var store, out var scheduler, out _, out _);
        A.CallTo(() => scheduler.RefreshAsync(A<ExecutorOptions>._, A<CancellationToken>._))
         .Returns(Task.CompletedTask);

        ExecutorOptions? saved = null;
        A.CallTo(() => store.SaveAsync(A<ExecutorOptions>._, A<CancellationToken>._))
         .Invokes(call => saved = call.GetArgument<ExecutorOptions>(0))
         .Returns(Task.CompletedTask);

        var model = new EndpointEditorModel
        {
            Name = "Ping",
            Url = "example.test",
            Cron = "0 0 * * * ?",
            Method = "PUT",
            Headers = "X-Test: value" + Environment.NewLine + "X-Other: two"
        };

        var result = await controller.Create(model, CancellationToken.None);

        result.Should()
              .BeOfType<RedirectToActionResult>();
        controller.TempData["StatusMessage"]
                  .Should()
                  .Be("Added endpoint 'Ping'.");
        saved.Should()
             .NotBeNull();
        saved!.Endpoints
              .Should()
              .ContainSingle();
        saved.Endpoints[0]
             .Url
             .Should()
             .Be("http://example.test/");
        saved.Endpoints[0]
             .Headers
             .Should()
             .ContainKey("X-Test");
    }

    [Fact]
    public async Task Delete_Post_RemovesEndpoint()
    {
        var options = new ExecutorOptions
        {
            Endpoints = new List<EndpointScheduleOptions>
            {
                new EndpointScheduleOptions
                {
                    Name = "DeleteMe",
                    Url = "https://example.test",
                    Cron = "0 0 * * * ?"
                }
            }
        };
        var controller = CreateController(options, new List<ApiServiceDescriptor>(), out var store, out var scheduler, out _, out _);
        A.CallTo(() => scheduler.RefreshAsync(A<ExecutorOptions>._, A<CancellationToken>._))
         .Returns(Task.CompletedTask);
        A.CallTo(() => store.SaveAsync(A<ExecutorOptions>._, A<CancellationToken>._))
         .Returns(Task.CompletedTask);

        var result = await controller.Delete("DeleteMe", CancellationToken.None);

        result.Should()
              .BeOfType<RedirectToActionResult>();
        controller.TempData["StatusMessage"]
                  .Should()
                  .Be("Deleted endpoint 'DeleteMe'.");
        options.Endpoints
               .Should()
               .BeEmpty();
    }

    [Fact]
    public async Task Edit_Get_MissingEndpoint_Redirects()
    {
        var controller = CreateController(new ExecutorOptions(), new List<ApiServiceDescriptor>(), out _, out _, out _, out _);

        var result = await controller.Edit("missing", CancellationToken.None);

        result.Should()
              .BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Edit_Post_Valid_UpdatesEndpoint()
    {
        var options = new ExecutorOptions
        {
            Endpoints = new List<EndpointScheduleOptions>
            {
                new EndpointScheduleOptions
                {
                    Name = "Existing",
                    Url = "https://example.test",
                    Cron = "0 0 * * * ?",
                    Method = "POST"
                }
            }
        };
        var controller = CreateController(options, new List<ApiServiceDescriptor>(), out var store, out var scheduler, out _, out _);
        A.CallTo(() => scheduler.RefreshAsync(A<ExecutorOptions>._, A<CancellationToken>._))
         .Returns(Task.CompletedTask);
        A.CallTo(() => store.SaveAsync(A<ExecutorOptions>._, A<CancellationToken>._))
         .Returns(Task.CompletedTask);

        var model = new EndpointEditorModel
        {
            Name = "Updated",
            Url = "updated.test",
            Cron = "0 1 * * * ?",
            Method = "PUT",
            Enabled = true
        };

        var result = await controller.Edit("Existing", "Existing", model, CancellationToken.None);

        result.Should()
              .BeOfType<RedirectToActionResult>();
        controller.TempData["StatusMessage"]
                  .Should()
                  .Be("Updated endpoint 'Updated'.");
        options.Endpoints[0]
               .Name
               .Should()
               .Be("Updated");
        options.Endpoints[0]
               .Url
               .Should()
               .Be("http://updated.test/");
    }

    [Fact]
    public async Task GetDiscoveredEndpoints_ReturnsMappedPayload()
    {
        var controller = CreateController(new ExecutorOptions(), new List<ApiServiceDescriptor>(), out _, out _, out var discovery, out _);
        var endpoints = new List<ExecutorDiscoveredEndpoint> { new ExecutorDiscoveredEndpoint("id", "POST", "/path", "Display", "Desc", true, "Group") };
        A.CallTo(() => discovery.GetEndpointsAsync("https://api.test", A<CancellationToken>._))
         .Returns(endpoints);

        var result = await controller.GetDiscoveredEndpoints("https://api.test", CancellationToken.None);

        var ok = result.Should()
                       .BeOfType<OkObjectResult>()
                       .Subject;
        var payload = ok.Value
                        .Should()
                        .BeAssignableTo<IEnumerable<object>>()
                        .Subject
                        .ToList();
        var item = payload.Should()
                          .ContainSingle()
                          .Subject;
        item.GetType()
            .GetProperty("id")!.GetValue(item)
            .Should()
            .Be("id");
        item.GetType()
            .GetProperty("groupName")!.GetValue(item)
            .Should()
            .Be("Group");
    }

    [Fact]
    public async Task GetDiscoveredEndpoints_WithEmptyBase_ReturnsBadRequest()
    {
        var controller = CreateController(new ExecutorOptions(), new List<ApiServiceDescriptor>(), out _, out _, out _, out _);

        var result = await controller.GetDiscoveredEndpoints(" ", CancellationToken.None);

        result.Should()
              .BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Index_ReturnsSortedEndpointsAndApis()
    {
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

        var controller = CreateController(options, apis, out _, out _, out _, out _);

        var result = await controller.Index(CancellationToken.None);

        var view = result.Should()
                         .BeOfType<ViewResult>()
                         .Subject;
        var model = view.Model
                        .Should()
                        .BeOfType<ExecutorConfigurationViewModel>()
                        .Subject;
        model.Endpoints
             .Should()
             .HaveCount(2);
        model.Endpoints[0]
             .Name
             .Should()
             .Be("Alpha");
        model.ApiServices
             .Should()
             .ContainSingle();
        model.ApiServices[0]
             .ServiceName
             .Should()
             .Be("mcm");
        model.Endpoints[1]
             .HeadersDisplay
             .Should()
             .Be("X-Test:value");
    }

    [Fact]
    public async Task Ping_Get_ReturnsPingDefaults()
    {
        var controller = CreateController(new ExecutorOptions(), new List<ApiServiceDescriptor>(), out _, out _, out _, out _);

        var result = await controller.Ping(CancellationToken.None);

        var view = result.Should()
                         .BeOfType<ViewResult>()
                         .Subject;
        var model = view.Model
                        .Should()
                        .BeOfType<ExecutorConfigurationViewModel>()
                        .Subject;
        model.Ping
             .Method
             .Should()
             .Be("GET");
    }

    [Fact]
    public async Task Ping_Post_InvalidUrl_ReturnsViewWithErrors()
    {
        var controller = CreateController(new ExecutorOptions(), new List<ApiServiceDescriptor>(), out _, out _, out _, out var pingService);

        var model = new EndpointPingModel
        {
            Url = "not-a-url",
            Method = "GET"
        };

        var result = await controller.Ping(model, CancellationToken.None);

        result.Should()
              .BeOfType<ViewResult>()
              .Which
              .ViewName
              .Should()
              .Be("Ping");
        A.CallTo(() => pingService.SendAsync(A<Uri>._, A<string>._, A<Dictionary<string, string>?>._, A<CancellationToken>._))
         .MustNotHaveHappened();
        controller.ModelState[nameof(EndpointPingModel.Url)]!.Errors
                  .Should()
                  .NotBeEmpty();
    }

    [Fact]
    public async Task Ping_Post_Success_ReturnsResultAndStatusMessage()
    {
        var controller = CreateController(new ExecutorOptions(), new List<ApiServiceDescriptor>(), out _, out _, out _, out var pingService);
        var body = new string('a', 2100);
        A.CallTo(() => pingService.SendAsync(A<Uri>._, A<string>._, A<Dictionary<string, string>?>._, A<CancellationToken>._))
         .Returns(new EndpointPingResult(true, 200, "OK", body, TimeSpan.FromMilliseconds(5)));

        var model = new EndpointPingModel
        {
            Url = "https://example.test",
            Method = "GET",
            Headers = "X-Test: value"
        };

        var result = await controller.Ping(model, CancellationToken.None);

        var view = result.Should()
                         .BeOfType<ViewResult>()
                         .Subject;
        var viewModel = view.Model
                            .Should()
                            .BeOfType<ExecutorConfigurationViewModel>()
                            .Subject;
        viewModel.PingResult
                 .Should()
                 .NotBeNull();
        viewModel.PingResult!.ResponseBody
                 .Should()
                 .EndWith("... (truncated)");
        controller.TempData["StatusMessage"]
                  .Should()
                  .Be("Ping succeeded with status 200 (5 ms).");
    }

    [Fact]
    public async Task Toggle_Post_UpdatesOptionsAndRefreshes()
    {
        var options = new ExecutorOptions { Enabled = false };
        var controller = CreateController(options, new List<ApiServiceDescriptor>(), out var store, out var scheduler, out _, out _);
        A.CallTo(() => store.SaveAsync(A<ExecutorOptions>._, A<CancellationToken>._))
         .Returns(Task.CompletedTask);
        A.CallTo(() => scheduler.RefreshAsync(A<ExecutorOptions>._, A<CancellationToken>._))
         .Returns(Task.CompletedTask);

        var result = await controller.Toggle(true, CancellationToken.None);

        options.Enabled
               .Should()
               .BeTrue();
        controller.TempData["StatusMessage"]
                  .Should()
                  .Be("Scheduler enabled.");
        result.Should()
              .BeOfType<RedirectToActionResult>();
    }

    private static HomeController CreateController(ExecutorOptions options, IReadOnlyList<ApiServiceDescriptor> apis, out IExecutorConfigurationStore store, out IExecutorSchedulerOrchestrator scheduler, out IApiEndpointDiscoveryService discovery, out IEndpointPingService pingService)
    {
        var storeFake = A.Fake<IExecutorConfigurationStore>();
        var schedulerFake = A.Fake<IExecutorSchedulerOrchestrator>();
        var discoveryFake = A.Fake<IApiEndpointDiscoveryService>();
        var pingServiceFake = A.Fake<IEndpointPingService>();
        var logger = A.Fake<ILogger<HomeController>>();

        A.CallTo(() => storeFake.GetAsync(A<CancellationToken>._))
         .Returns(options);
        A.CallTo(() => discoveryFake.GetApisAsync(A<CancellationToken>._))
         .Returns(apis);

        store = storeFake;
        scheduler = schedulerFake;
        discovery = discoveryFake;
        pingService = pingServiceFake;

        var controller = new HomeController(storeFake, schedulerFake, discoveryFake, pingServiceFake, logger);
        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        var tempDataProvider = A.Fake<ITempDataProvider>();
        A.CallTo(() => tempDataProvider.LoadTempData(httpContext))
         .Returns(new Dictionary<string, object?>());
        controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);

        return controller;
    }
}