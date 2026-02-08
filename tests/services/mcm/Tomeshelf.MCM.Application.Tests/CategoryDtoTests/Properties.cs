using Shouldly;
using Tomeshelf.MCM.Application.Mcm;

namespace Tomeshelf.MCM.Application.Tests.CategoryDtoTests;

public class Properties
{
    [Fact]
    public void CanSetAndGetValues()
    {
        // Arrange
        var id = "category-id-1";
        var name = "Category Name";
        var color = "#FFFFFF";

        // Act
        var dto = new CategoryDto
        {
            Id = id,
            Name = name,
            Color = color
        };

        // Assert
        dto.Id.ShouldBe(id);
        dto.Name.ShouldBe(name);
        dto.Color.ShouldBe(color);
    }

    [Fact]
    public void DefaultsAreNull()
    {
        // Arrange
        var dto = new CategoryDto();

        var id = dto.Id;
        var name = dto.Name;
        // Act
        var color = dto.Color;

        // Assert
        id.ShouldBeNull();
        name.ShouldBeNull();
        color.ShouldBeNull();
    }
}