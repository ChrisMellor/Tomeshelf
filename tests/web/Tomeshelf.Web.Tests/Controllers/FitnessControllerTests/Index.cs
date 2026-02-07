using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Tomeshelf.Web.Controllers;
using Tomeshelf.Web.Models.Fitness;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Tests.Controllers.FitnessControllerTests;

public class Index
{
    [Fact]
    public async Task WhenOverviewHasSummary_ReturnsPopulatedModel()
    {
        // Arrange
        var api = A.Fake<IFitbitApi>();
        var logger = A.Fake<ILogger<FitnessController>>();
        var date = "2020-01-02";

        var overview = new FitbitOverviewModel
        {
            Daily = new FitbitDashboardModel
            {
                Date = date,
                Activity = new FitbitActivityModel { Steps = 250 }
            },
            Last7Days = new FitbitOverviewRangeModel { Items = new List<FitbitOverviewDayModel>() },
            Last30Days = new FitbitOverviewRangeModel { Items = new List<FitbitOverviewDayModel>() }
        };

        A.CallTo(() => api.GetOverviewAsync(date, A<bool>._, A<string>._, A<CancellationToken>._))
         .Returns(Task.FromResult(overview));

        var controller = CreateController(api, logger);

        // Act
        var result = await controller.Index(date, false, "kg", CancellationToken.None);

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<FitnessDashboardViewModel>();
        model.Summary.ShouldNotBeNull();
        model.Summary!.Activity.Steps.ShouldBe(250);
        model.ErrorMessage.ShouldBeNull();
        model.HasData.ShouldBeTrue();
        model.Last7Days.HasData.ShouldBeFalse();
        model.Last30Days.HasData.ShouldBeFalse();
    }

    [Fact]
    public async Task WhenOverviewMissing_ReturnsEmptyModel()
    {
        // Arrange
        var api = A.Fake<IFitbitApi>();
        var logger = A.Fake<ILogger<FitnessController>>();
        var date = "2020-01-01";

        A.CallTo(() => api.GetOverviewAsync(date, A<bool>._, A<string>._, A<CancellationToken>._))
         .Returns(Task.FromResult<FitbitOverviewModel>(null));

        var controller = CreateController(api, logger);

        // Act
        var result = await controller.Index(date, false, "lb", CancellationToken.None);

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<FitnessDashboardViewModel>();
        model.SelectedDate.ShouldBe(date);
        model.PreviousDate.ShouldBe("2019-12-31");
        model.NextDate.ShouldBe("2020-01-02");
        model.Unit.ShouldBe(WeightUnit.Pounds);
        model.ErrorMessage.ShouldBe("No Fitbit data is available for the selected date.");
        model.HasData.ShouldBeFalse();

        A.CallTo(() => api.GetOverviewAsync(date, false, A<string>._, A<CancellationToken>._))
         .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task WhenRangeDataPresent_BuildsMetrics()
    {
        // Arrange
        var api = A.Fake<IFitbitApi>();
        var logger = A.Fake<ILogger<FitnessController>>();
        var date = "2020-01-04";

        var overview = new FitbitOverviewModel
        {
            Daily = new FitbitDashboardModel
            {
                Date = date,
                Activity = new FitbitActivityModel { Steps = 1 }
            },
            Last7Days = new FitbitOverviewRangeModel
            {
                Items = new List<FitbitOverviewDayModel>
                {
                    new FitbitOverviewDayModel
                    {
                        Date = "2020-01-03",
                        WeightKg = 10,
                        Steps = 1000,
                        SleepHours = 7.5,
                        NetCalories = 200
                    },
                    new FitbitOverviewDayModel
                    {
                        Date = "2020-01-04",
                        WeightKg = 11,
                        Steps = 1100,
                        SleepHours = 7.0,
                        NetCalories = 180
                    }
                }
            },
            Last30Days = new FitbitOverviewRangeModel { Items = new List<FitbitOverviewDayModel>() }
        };

        A.CallTo(() => api.GetOverviewAsync(date, A<bool>._, A<string>._, A<CancellationToken>._))
         .Returns(Task.FromResult(overview));

        var controller = CreateController(api, logger);

        // Act
        var result = await controller.Index(date, false, "lb", CancellationToken.None);

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<FitnessDashboardViewModel>();
        model.Last7Days.HasData.ShouldBeTrue();
        model.Last7Days.Metrics.Count.ShouldBe(4);
        var weightValue = model.Last7Days.Metrics[0].Values[0]!.Value;
        weightValue.ShouldBeInRange(22.0362, 22.0562);
        model.Last7Days.Metrics[1].Values[0]!.Value.ShouldBe(1000);
    }

    [Fact]
    public async Task WhenTrendDataPresentButDailyMissing_ShowsDailyMissingMessage()
    {
        // Arrange
        var api = A.Fake<IFitbitApi>();
        var logger = A.Fake<ILogger<FitnessController>>();
        var date = "2020-01-03";

        var overview = new FitbitOverviewModel
        {
            Daily = new FitbitDashboardModel { Date = date },
            Last7Days = new FitbitOverviewRangeModel
            {
                Items = new List<FitbitOverviewDayModel>
                {
                    new FitbitOverviewDayModel
                    {
                        Date = "2020-01-02",
                        Steps = 1200
                    }
                }
            },
            Last30Days = new FitbitOverviewRangeModel { Items = new List<FitbitOverviewDayModel>() }
        };

        A.CallTo(() => api.GetOverviewAsync(date, A<bool>._, A<string>._, A<CancellationToken>._))
         .Returns(Task.FromResult(overview));

        var controller = CreateController(api, logger);

        // Act
        var result = await controller.Index(date, false, "kg", CancellationToken.None);

        // Assert
        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<FitnessDashboardViewModel>();
        model.Summary.ShouldBeNull();
        model.ErrorMessage.ShouldBe("No daily Fitbit data is available for the selected date.");
        model.HasData.ShouldBeFalse();
        model.Last7Days.HasData.ShouldBeTrue();
    }

    private static FitnessController CreateController(IFitbitApi api, ILogger<FitnessController> logger)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("example.test");

        var urlHelper = A.Fake<IUrlHelper>();
        A.CallTo(() => urlHelper.Action(A<UrlActionContext>._))
         .Returns("https://example.test/fitness");
        A.CallTo(() => urlHelper.ActionContext)
         .Returns(new ActionContext(httpContext, new RouteData(), new ActionDescriptor()));

        return new FitnessController(api, logger)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            Url = urlHelper
        };
    }
}
