using System;
using Shouldly;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.FitnessDashboardViewModelTests;

public class IsToday
{
    [Fact]
    public void WhenSelectedDateMatchesToday_ReturnsTrue()
    {
        var today = DateOnly.FromDateTime(DateTime.Today)
                            .ToString("yyyy-MM-dd");

        var model = FitnessDashboardViewModel.Empty(today, WeightUnit.Stones);

        model.IsToday.ShouldBeTrue();
        model.CanRefresh.ShouldBeFalse();
    }
}