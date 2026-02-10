using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Fitbit.Api.Controllers;
using Tomeshelf.Fitbit.Application;
using Tomeshelf.Fitbit.Application.Features.Dashboard.Queries;
using Tomeshelf.Fitbit.Application.Features.Overview.Models;
using Tomeshelf.Fitbit.Application.Features.Overview.Queries;

namespace Tomeshelf.Fitbit.Api.Tests.TestUtilities;

public static class FitbitControllerTestHarness
{
    /// <summary>
    ///     Creates the controller.
    /// </summary>
    /// <param name="dashboardHandler">The dashboard handler.</param>
    /// <param name="overviewHandler">The overview handler.</param>
    /// <returns>The result of the operation.</returns>
    public static FitbitController CreateController(IQueryHandler<GetFitbitDashboardQuery, FitbitDashboardDto> dashboardHandler, IQueryHandler<GetFitbitOverviewQuery, FitbitOverviewDto> overviewHandler)
    {
        var options = new FitbitOptions
        {
            ClientId = "client",
            ClientSecret = "secret",
            CallbackBaseUri = "https://example.test",
            CallbackPath = "/api/fitbit/auth/callback"
        };

        var controller = new FitbitController(dashboardHandler, overviewHandler, new TestOptionsMonitor<FitbitOptions>(options), NullLogger<FitbitController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        controller.Url = new TestUrlHelper(controller.ControllerContext, _ => "/authorize");

        return controller;
    }

    /// <summary>
    ///     Creates the snapshot.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>The result of the operation.</returns>
    public static FitbitDashboardDto CreateSnapshot(string date)
    {
        return new FitbitDashboardDto
        {
            Date = date,
            Weight = new FitbitWeightSummaryDto
            {
                StartingWeightKg = 80,
                CurrentWeightKg = 80,
                ChangeKg = 0
            },
            Calories = new FitbitCaloriesSummaryDto
            {
                IntakeCalories = 2000,
                BurnedCalories = 1800,
                NetCalories = -200,
                CarbsGrams = 200,
                FatGrams = 100,
                FiberGrams = 20,
                ProteinGrams = 15,
                SodiumMilligrams = 30
            },
            Sleep = new FitbitSleepSummaryDto
            {
                TotalSleepHours = 7,
                TotalAwakeHours = 1,
                EfficiencyPercentage = 85,
                Bedtime = "22:00",
                WakeTime = "06:00",
                Levels = new FitbitSleepLevelsDto
                {
                    DeepMinutes = 60,
                    LightMinutes = 240,
                    RemMinutes = 90,
                    WakeMinutes = 30
                }
            },
            Activity = new FitbitActivitySummaryDto(5000, 4.2, 10)
        };
    }

    private sealed class TestUrlHelper : IUrlHelper
    {
        private readonly Func<UrlActionContext, string?> _action;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestUrlHelper" /> class.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        /// <param name="action">The action.</param>
        public TestUrlHelper(ActionContext actionContext, Func<UrlActionContext, string?> action)
        {
            ActionContext = actionContext;
            _action = action;
        }

        public ActionContext ActionContext { get; }

        /// <summary>
        ///     Actions.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        /// <returns>The result of the operation.</returns>
        public string? Action(UrlActionContext actionContext)
        {
            return _action(actionContext);
        }

        /// <summary>
        ///     Contents.
        /// </summary>
        /// <param name="contentPath">The content path.</param>
        /// <returns>The result of the operation.</returns>
        public string? Content(string? contentPath)
        {
            return contentPath;
        }

        /// <summary>
        ///     Determines whether the specified url is a local URL.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <returns>True if the condition is met; otherwise, false.</returns>
        public bool IsLocalUrl(string? url)
        {
            return true;
        }

        /// <summary>
        ///     Links.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <param name="values">The values.</param>
        /// <returns>The result of the operation.</returns>
        public string? Link(string? routeName, object? values)
        {
            return null;
        }

        /// <summary>
        ///     Routes the url.
        /// </summary>
        /// <param name="routeContext">The route context.</param>
        /// <returns>The result of the operation.</returns>
        public string? RouteUrl(UrlRouteContext routeContext)
        {
            return null;
        }
    }
}
