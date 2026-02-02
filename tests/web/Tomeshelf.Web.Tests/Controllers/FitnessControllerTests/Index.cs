using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        var view = result.Should()
                         .BeOfType<ViewResult>()
                         .Subject;
        var model = view.Model
                        .Should()
                        .BeOfType<FitnessDashboardViewModel>()
                        .Subject;
        model.Summary
             .Should()
             .NotBeNull();
        model.Summary
             .Activity
             .Steps
             .Should()
             .Be(250);
        model.ErrorMessage
             .Should()
             .BeNull();
        model.HasData
             .Should()
             .BeTrue();
        model.Last7Days
             .HasData
             .Should()
             .BeFalse();
        model.Last30Days
             .HasData
             .Should()
             .BeFalse();
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
        var view = result.Should()
                         .BeOfType<ViewResult>()
                         .Subject;
        var model = view.Model
                        .Should()
                        .BeOfType<FitnessDashboardViewModel>()
                        .Subject;
        model.SelectedDate
             .Should()
             .Be(date);
        model.PreviousDate
             .Should()
             .Be("2019-12-31");
        model.NextDate
             .Should()
             .Be("2020-01-02");
        model.Unit
             .Should()
             .Be(WeightUnit.Pounds);
        model.ErrorMessage
             .Should()
             .Be("No Fitbit data is available for the selected date.");
        model.HasData
             .Should()
             .BeFalse();

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
                    new()
                    {
                        Date = "2020-01-03",
                        WeightKg = 10,
                        Steps = 1000,
                        SleepHours = 7.5,
                        NetCalories = 200
                    },
                    new()
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
        var view = result.Should()
                         .BeOfType<ViewResult>()
                         .Subject;
        var model = view.Model
                        .Should()
                        .BeOfType<FitnessDashboardViewModel>()
                        .Subject;
        model.Last7Days
             .HasData
             .Should()
             .BeTrue();
        model.Last7Days
             .Metrics
             .Should()
             .HaveCount(4);
        model.Last7Days
             .Metrics[0]
             .Values[0]!.Value
             .Should()
             .BeApproximately(22.0462, 0.01);
        model.Last7Days
             .Metrics[1]
             .Values[0]!.Value
             .Should()
             .Be(1000);
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
                    new()
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
        var view = result.Should()
                         .BeOfType<ViewResult>()
                         .Subject;
        var model = view.Model
                        .Should()
                        .BeOfType<FitnessDashboardViewModel>()
                        .Subject;
        model.Summary
             .Should()
             .BeNull();
        model.ErrorMessage
             .Should()
             .Be("No daily Fitbit data is available for the selected date.");
        model.HasData
             .Should()
             .BeFalse();
        model.Last7Days
             .HasData
             .Should()
             .BeTrue();
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
