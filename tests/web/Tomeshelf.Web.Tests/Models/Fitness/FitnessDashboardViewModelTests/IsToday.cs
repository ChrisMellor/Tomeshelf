using System;
using FluentAssertions;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.FitnessDashboardViewModelTests;

public class IsToday
{
    [Fact]
    public void WhenSelectedDateMatchesToday_ReturnsTrue()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");

        // Act
        var model = FitnessDashboardViewModel.Empty(today, WeightUnit.Stones);

        // Assert
        model.IsToday.Should().BeTrue();
        model.CanRefresh.Should().BeFalse();
    }
}
