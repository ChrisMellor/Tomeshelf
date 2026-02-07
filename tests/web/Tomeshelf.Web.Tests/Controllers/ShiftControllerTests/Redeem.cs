using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
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
        var view = result.ShouldBeOfType<ViewResult>();
        view.ViewName.ShouldBe("Index");
        var viewModel = view.Model.ShouldBeOfType<ShiftIndexViewModel>();
        viewModel.ErrorMessage.ShouldBe("Redeem failed: boom");
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
        var view = result.ShouldBeOfType<ViewResult>();
        view.ViewName.ShouldBe("Index");
        var viewModel = view.Model.ShouldBeOfType<ShiftIndexViewModel>();
        viewModel.Code.ShouldBe(model.Code);
        controller.ModelState.IsValid.ShouldBeFalse();
        var errors = controller.ModelState[nameof(ShiftIndexViewModel.Code)].Errors;
        errors.ShouldHaveSingleItem();
        errors[0].ErrorMessage.ShouldBe("Enter a SHiFT code to redeem.");
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
        var view = result.ShouldBeOfType<ViewResult>();
        view.ViewName.ShouldBe("Index");
        var viewModel = view.Model.ShouldBeOfType<ShiftIndexViewModel>();
        viewModel.Code.ShouldBe("ABC");
        viewModel.Response.ShouldBeSameAs(response);
        A.CallTo(() => api.RedeemCodeAsync("ABC", A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }
}
