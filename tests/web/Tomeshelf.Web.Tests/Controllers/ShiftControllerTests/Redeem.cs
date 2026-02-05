using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Tomeshelf.Web.Controllers;
using Tomeshelf.Web.Models.Shift;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Tests.Controllers.ShiftControllerTests;

public class Redeem
{
    [Fact]
    public async Task WhenApiThrows_ReturnsErrorMessage()
    {
        // Arrange
        var api = A.Fake<IShiftApi>();
        A.CallTo(() => api.RedeemCodeAsync("ABC", A<CancellationToken>._))
         .Throws(new Exception("boom"));

        var controller = new ShiftController(api);
        var model = new ShiftIndexViewModel { Code = "ABC" };

        // Act
        var result = await controller.Redeem(model, CancellationToken.None);

        // Assert
        var view = result.Should()
                         .BeOfType<ViewResult>()
                         .Subject;
        view.ViewName
            .Should()
            .Be("Index");
        var viewModel = view.Model
                            .Should()
                            .BeOfType<ShiftIndexViewModel>()
                            .Subject;
        viewModel.ErrorMessage
                 .Should()
                 .Be("Redeem failed: boom");
    }

    [Fact]
    public async Task WhenCodeMissing_ReturnsValidationError()
    {
        // Arrange
        var api = A.Fake<IShiftApi>();
        var controller = new ShiftController(api);
        var model = new ShiftIndexViewModel { Code = "  " };

        // Act
        var result = await controller.Redeem(model, CancellationToken.None);

        // Assert
        var view = result.Should()
                         .BeOfType<ViewResult>()
                         .Subject;
        view.ViewName
            .Should()
            .Be("Index");
        var viewModel = view.Model
                            .Should()
                            .BeOfType<ShiftIndexViewModel>()
                            .Subject;
        viewModel.Code
                 .Should()
                 .Be(model.Code);
        controller.ModelState
                  .IsValid
                  .Should()
                  .BeFalse();
        controller.ModelState[nameof(ShiftIndexViewModel.Code)]
                  .Errors
                  .Should()
                  .ContainSingle();
        controller.ModelState[nameof(ShiftIndexViewModel.Code)]
                  .Errors[0]
                  .ErrorMessage
                  .Should()
                  .Be("Enter a SHiFT code to redeem.");
        A.CallTo(() => api.RedeemCodeAsync(A<string>._, A<CancellationToken>._))
         .MustNotHaveHappened();
    }

    [Fact]
    public async Task WhenCodeValid_ReturnsResponse()
    {
        // Arrange
        var api = A.Fake<IShiftApi>();
        var response = new RedeemResponseModel(new RedeemSummaryModel(1, 1, 0), new[] { new RedeemResultModel(1, "user@example.com", "steam", true, null, null) });
        A.CallTo(() => api.RedeemCodeAsync("ABC", A<CancellationToken>._))
         .Returns(response);

        var controller = new ShiftController(api);
        var model = new ShiftIndexViewModel { Code = "  ABC  " };

        // Act
        var result = await controller.Redeem(model, CancellationToken.None);

        // Assert
        var view = result.Should()
                         .BeOfType<ViewResult>()
                         .Subject;
        view.ViewName
            .Should()
            .Be("Index");
        var viewModel = view.Model
                            .Should()
                            .BeOfType<ShiftIndexViewModel>()
                            .Subject;
        viewModel.Code
                 .Should()
                 .Be("ABC");
        viewModel.Response
                 .Should()
                 .Be(response);
        A.CallTo(() => api.RedeemCodeAsync("ABC", A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}