using System.Threading;
using System.Threading.Tasks;

namespace Tomeshelf.SHiFT.Application.Abstractions.Messaging;

public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
}
