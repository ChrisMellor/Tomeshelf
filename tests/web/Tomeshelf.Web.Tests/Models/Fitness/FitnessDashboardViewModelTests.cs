using System;
using Tomeshelf.Web.Models.Fitness;
using Xunit;

namespace Tomeshelf.Web.Tests.Models.Fitness;

public class FitnessDashboardViewModelTests
{
    [Fact]
    public void Empty_SetsDatesAndFlags()
    {
        // Arrange
        var selectedDate = "2020-01-01";

        // Act
        var model = FitnessDashboardViewModel.Empty(selectedDate, WeightUnit.Pounds, "error");

        // Assert
        Assert.Equal(selectedDate, model.SelectedDate);
        Assert.Equal("2020-01-02", model.NextDate);
        Assert.Equal("2019-12-31", model.PreviousDate);
        Assert.False(model.HasData);
        Assert.Equal("error", model.ErrorMessage);
        Assert.Equal(WeightUnit.Pounds, model.Unit);
    }

    [Fact]
    public void IsToday_WhenSelectedDateMatchesToday()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");

        // Act
        var model = FitnessDashboardViewModel.Empty(today, WeightUnit.Stones);

        // Assert
        Assert.True(model.IsToday);
        Assert.False(model.CanRefresh);
    }
}
