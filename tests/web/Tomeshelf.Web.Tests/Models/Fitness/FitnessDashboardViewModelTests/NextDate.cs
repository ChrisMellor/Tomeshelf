using System;
using Shouldly;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.FitnessDashboardViewModelTests;

public class NextDate
{
    /// <summary>
    ///     The next date is null when the selected date is today.
    /// </summary>
    [Fact]
    public void WhenSelectedDateIsToday_NextDateIsNull()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today)
                            .ToString("yyyy-MM-dd");

        // Act
        var model = FitnessDashboardViewModel.Empty(today, WeightUnit.Stones);

        // Assert
        model.NextDate.ShouldBeNull();
        model.PreviousDate.ShouldBe(DateOnly.FromDateTime(DateTime.Today)
                                            .AddDays(-1)
                                            .ToString("yyyy-MM-dd"));
    }
}