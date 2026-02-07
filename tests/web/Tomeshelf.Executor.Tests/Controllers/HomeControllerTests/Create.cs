using System;
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
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.Executor.Tests.Controllers.HomeControllerTests;

public class Create
{
    [Fact]
    public async Task Get_ReturnsEditorDefaults()
    {
        // Arrange
        var controller = HomeControllerTestHarness.CreateController(new ExecutorOptions(), new List<ApiServiceDescriptor>(), out _, out _, out _, out _);

        // Act
        var result = await controller.Create(CancellationToken.None);

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<ExecutorConfigurationViewModel>();
        model.Editor.Enabled.ShouldBeTrue();
        model.Editor.Method.ShouldBe("POST");
    }

    [Fact]
    public async Task Post_DuplicateName_ReturnsViewWithError()
    {
        // Arrange
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
        var controller = HomeControllerTestHarness.CreateController(options, new List<ApiServiceDescriptor>(), out var store, out _, out _, out _);

        var model = new EndpointEditorModel
        {
            Name = "existing",
            Url = "https://new.test",
            Cron = "0 0 * * * ?",
            Method = "POST"
        };

        // Act
        var result = await controller.Create(model, CancellationToken.None);

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        view.ViewName.ShouldBe("Create");
        controller.ModelState[nameof(EndpointEditorModel.Name)]!.Errors.ShouldNotBeEmpty();
        A.CallTo(() => store.SaveAsync(A<ExecutorOptions>._, A<CancellationToken>._))
         .MustNotHaveHappened();
    }

    [Fact]
    public async Task Post_InvalidModel_ReturnsView()
    {
        // Arrange
        var options = new ExecutorOptions();
        var controller = HomeControllerTestHarness.CreateController(options, new List<ApiServiceDescriptor>(), out var store, out _, out _, out _);
        controller.ModelState.AddModelError("Name", "Required");

        var model = new EndpointEditorModel { Name = "Endpoint" };

        // Act
        var result = await controller.Create(model, CancellationToken.None);

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        view.ViewName.ShouldBe("Create");
        A.CallTo(() => store.SaveAsync(A<ExecutorOptions>._, A<CancellationToken>._))
         .MustNotHaveHappened();
    }

    [Fact]
    public async Task Post_Valid_AddsEndpointAndRedirects()
    {
        // Arrange
        var options = new ExecutorOptions();
        var controller = HomeControllerTestHarness.CreateController(options, new List<ApiServiceDescriptor>(), out var store, out var scheduler, out _, out _);
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

        // Act
        var result = await controller.Create(model, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<RedirectToActionResult>();
        controller.TempData["StatusMessage"].ShouldBe("Added endpoint 'Ping'.");
        saved.ShouldNotBeNull();
        var endpoint = saved!.Endpoints.ShouldHaveSingleItem();
        endpoint.Url.ShouldBe("http://example.test/");
        endpoint.Headers.ShouldNotBeNull();
        endpoint.Headers!.ContainsKey("X-Test").ShouldBeTrue();
    }
}
