using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Velo.CQRS.Queries
{
    public interface IQueryProcessor<in TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        Task<TResult> Process(TQuery query, CancellationToken cancellationToken);
    }
}

#nullable restore