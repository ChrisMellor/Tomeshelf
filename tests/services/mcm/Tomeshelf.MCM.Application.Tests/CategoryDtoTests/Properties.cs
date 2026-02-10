using Shouldly;
using Tomeshelf.MCM.Application.Mcm;

namespace Tomeshelf.MCM.Application.Tests.CategoryDtoTests;

public class Properties
{
    /// <summary>
    ///     Determines whether the current instance can set and get values.
    /// </summary>
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

    /// <summary>
    ///     Defaults the are null.
    /// </summary>
    [Fact]
    public void DefaultsAreNull()
    {
        // Arrange
        var dto = new CategoryDto();

        // Act
        var id = dto.Id;
        var name = dto.Name;
        var color = dto.Color;

        // Assert
        id.ShouldBeNull();
        name.ShouldBeNull();
        color.ShouldBeNull();
    }
}