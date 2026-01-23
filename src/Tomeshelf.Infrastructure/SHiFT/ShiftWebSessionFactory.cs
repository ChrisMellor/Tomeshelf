using Tomeshelf.Application.Shared.Abstractions.SHiFT;

namespace Tomeshelf.Infrastructure.Shared.SHiFT;

public sealed class ShiftWebSessionFactory : IShiftWebSessionFactory
{
    /// <inheritdoc />
    public IShiftWebSession Create()
    {
        return new ShiftWebSession();
    }
}