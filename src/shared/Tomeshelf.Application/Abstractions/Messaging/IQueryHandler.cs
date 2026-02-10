using System.Threading;
using System.Threading.Tasks;

namespace Tomeshelf.Application.Shared.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    /// <summary>
    ///     Handles.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the operation result.</returns>
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
}