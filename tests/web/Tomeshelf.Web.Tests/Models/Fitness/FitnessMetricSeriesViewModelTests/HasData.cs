using System.Collections.Generic;
using Bogus;
using Shouldly;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.FitnessMetricSeriesViewModelTests;

public class HasData
{
    /// <summary>
    ///     Returns false when the all values are null.
    /// </summary>
    [Fact]
    public void WhenAllValuesNull_ReturnsFalse()
    {
        // Arrange
        var faker = new Faker();
        var model = new FitnessMetricSeriesViewModel
        {
            Key = faker.Random.Word(),
            Title = faker.Lorem.Word(),
            Unit = faker.Random.Word(),
            Values = new List<double?>
            {
                null,
                null
            }
        };

        // Act
        var hasData = model.HasData;

        // Assert
        hasData.ShouldBeFalse();
    }

    /// <summary>
    ///     Returns true when the any value is present.
    /// </summary>
    [Fact]
    public void WhenAnyValuePresent_ReturnsTrue()
    {
        // Arrange
        var faker = new Faker();
        var model = new FitnessMetricSeriesViewModel
        {
            Key = faker.Random.Word(),
            Title = faker.Lorem.Word(),
            Unit = faker.Random.Word(),
            Values = new List<double?>
            {
                null,
                10,
                null
            }
        };

        // Act
        var hasData = model.HasData;

        // Assert
        hasData.ShouldBeTrue();
    }

    /// <summary>
    ///     Returns false when the values are null.
    /// </summary>
    [Fact]
    public void WhenValuesNull_ReturnsFalse()
    {
        // Arrange
        var faker = new Faker();
        var model = new FitnessMetricSeriesViewModel
        {
            Key = faker.Random.Word(),
            Title = faker.Lorem.Word(),
            Unit = faker.Random.Word(),
            Values = null
        };

        // Act
        var hasData = model.HasData;

        // Assert
        hasData.ShouldBeFalse();
    }
}