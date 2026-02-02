using System;
using FluentAssertions;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.FitnessDashboardViewModelTests;

public class NextDate
{
    [Fact]
    public void WhenSelectedDateIsToday_NextDateIsNull()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");

        // Act
        var model = FitnessDashboardViewModel.Empty(today, WeightUnit.Stones);

        // Assert
        model.NextDate.Should().BeNull();
        model.PreviousDate.Should().Be(DateOnly.FromDateTime(DateTime.Today).AddDays(-1).ToString("yyyy-MM-dd"));
    }
}
