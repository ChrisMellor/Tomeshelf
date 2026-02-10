using System.Threading;
using System.Threading.Tasks;

namespace Tomeshelf.Application.Shared.Abstractions.Messaging;

public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
}