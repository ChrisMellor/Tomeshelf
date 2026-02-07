using Tomeshelf.MCM.Infrastructure.Responses;

namespace Tomeshelf.MCM.Infrastructure.Tests.McmEventResponseTests;

public class GlobalCategory
{
    [Fact]
    public void CanSetAndGetValues()
    {
        // Arrange
        var id = "cat-id-1";
        var name = "Category A";
        var colour = "#FF0000";

        // Act
        var globalCategory = new McmEventResponse.GlobalCategory
        {
            Id = id,
            Name = name,
            Colour = colour
        };

        // Assert
        globalCategory.Id.ShouldBe(id);
        globalCategory.Name.ShouldBe(name);
        globalCategory.Colour.ShouldBe(colour);
    }
}
