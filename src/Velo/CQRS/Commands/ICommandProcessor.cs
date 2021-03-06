using System.Threading;
using System.Threading.Tasks;

namespace Velo.CQRS.Commands
{
    public interface ICommandProcessor<in TCommand>
        where TCommand: ICommand
    {
        Task Process(TCommand command, CancellationToken cancellationToken);
    }
}