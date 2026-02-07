using System;
using Shouldly;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.FitnessDashboardViewModelTests;

public class NextDate
{
    [Fact]
    public void WhenSelectedDateIsToday_NextDateIsNull()
    {
        var today = DateOnly.FromDateTime(DateTime.Today)
                            .ToString("yyyy-MM-dd");

        var model = FitnessDashboardViewModel.Empty(today, WeightUnit.Stones);

        model.NextDate.ShouldBeNull();
        model.PreviousDate.ShouldBe(DateOnly.FromDateTime(DateTime.Today)
                                            .AddDays(-1)
                                            .ToString("yyyy-MM-dd"));
    }
}