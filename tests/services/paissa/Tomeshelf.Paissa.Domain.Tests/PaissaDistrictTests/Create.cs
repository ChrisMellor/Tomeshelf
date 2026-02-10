using Shouldly;
using Tomeshelf.Paissa.Domain.Entities;

namespace Tomeshelf.Paissa.Domain.Tests.PaissaDistrictTests;

public class Create
{
    /// <summary>
    ///     Throws argument exception when the name is empty.
    /// </summary>
    [Fact]
    public void EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var plots = new List<PaissaPlot>();

        // Act
        var exception = Should.Throw<ArgumentException>(() => PaissaDistrict.Create(1, string.Empty, plots));

        // Assert
        exception.ShouldNotBeNull();
    }

    /// <summary>
    ///     Throws argument out of range exception when the ID is invalid.
    /// </summary>
    [Fact]
    public void InvalidId_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var plots = new List<PaissaPlot>();

        // Act
        var exception = Should.Throw<ArgumentOutOfRangeException>(() => PaissaDistrict.Create(0, "Mist", plots));

        // Assert
        exception.ShouldNotBeNull();
    }

    /// <summary>
    ///     Throws argument exception when the name is null.
    /// </summary>
    [Fact]
    public void NullName_ThrowsArgumentException()
    {
        // Arrange
        var plots = new List<PaissaPlot>();

        // Act
        var exception = Should.Throw<ArgumentException>(() => PaissaDistrict.Create(1, null!, plots));

        // Assert
        exception.ShouldNotBeNull();
    }

    /// <summary>
    ///     Throws argument null exception when the plots are null.
    /// </summary>
    [Fact]
    public void NullPlots_ThrowsArgumentNullException()
    {
        // Arrange
        // Act
        var exception = Should.Throw<ArgumentNullException>(() => PaissaDistrict.Create(1, "Mist", null!));

        // Assert
        exception.ShouldNotBeNull();
    }

    /// <summary>
    ///     Throws argument exception when the plots contain null entry.
    /// </summary>
    [Fact]
    public void PlotsWithNullEntry_ThrowsArgumentException()
    {
        // Arrange
        var plots = new List<PaissaPlot> { null! };

        // Act
        var exception = Should.Throw<ArgumentException>(() => PaissaDistrict.Create(1, "Mist", plots));

        // Assert
        exception.ShouldNotBeNull();
    }

    /// <summary>
    ///     Returns paissa district when the parameters are valid.
    /// </summary>
    [Fact]
    public void ValidParameters_ReturnsPaissaDistrict()
    {
        // Arrange
        var plots = new List<PaissaPlot>();

        // Act
        var district = PaissaDistrict.Create(1, "Mist", plots);

        // Assert
        district.ShouldNotBeNull();
        district.Id.ShouldBe(1);
        district.Name.ShouldBe("Mist");
        district.OpenPlots.ShouldBeEmpty();
    }
}