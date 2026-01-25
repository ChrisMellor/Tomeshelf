using System.Threading;
using System.Threading.Tasks;

namespace Tomeshelf.SHiFT.Application.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
}
