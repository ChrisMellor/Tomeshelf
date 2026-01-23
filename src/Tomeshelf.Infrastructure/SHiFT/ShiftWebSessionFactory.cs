using Tomeshelf.Application.Abstractions.SHiFT;

namespace Tomeshelf.Infrastructure.SHiFT;

public sealed class ShiftWebSessionFactory : IShiftWebSessionFactory
{
    /// <inheritdoc />
    public IShiftWebSession Create()
    {
        return new ShiftWebSession();
    }
}