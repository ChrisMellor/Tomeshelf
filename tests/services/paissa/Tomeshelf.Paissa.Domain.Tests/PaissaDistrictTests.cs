using System;
using System.Collections.Generic;
using Xunit;
using Tomeshelf.Paissa.Domain.Entities;

namespace Tomeshelf.Paissa.Domain.Tests;

public class PaissaDistrictTests
{
    [Fact]
    public void Create_ValidParameters_ReturnsPaissaDistrict()
    {
        // Arrange
        var plots = new List<PaissaPlot>();
        
        // Act
        var district = PaissaDistrict.Create(1, "Mist", plots);

        // Assert
        Assert.NotNull(district);
        Assert.Equal(1, district.Id);
        Assert.Equal("Mist", district.Name);
        Assert.Empty(district.OpenPlots);
    }

    [Fact]
    public void Create_InvalidId_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var plots = new List<PaissaPlot>();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => PaissaDistrict.Create(0, "Mist", plots));
    }

    [Fact]
    public void Create_NullName_ThrowsArgumentException()
    {
        // Arrange
        var plots = new List<PaissaPlot>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => PaissaDistrict.Create(1, (string)null!, plots));
    }

    [Fact]
    public void Create_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var plots = new List<PaissaPlot>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => PaissaDistrict.Create(1, "", plots));
    }

    [Fact]
    public void Create_NullPlots_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PaissaDistrict.Create(1, "Mist", (List<PaissaPlot>)null!));
    }

    [Fact]
    public void Create_PlotsWithNullEntry_ThrowsArgumentException()
    {
        // Arrange
        var plots = new List<PaissaPlot> { (PaissaPlot)null! };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => PaissaDistrict.Create(1, "Mist", plots));
    }
}
