using System.Threading;
using System.Threading.Tasks;

namespace Tomeshelf.Application.Shared.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
}
