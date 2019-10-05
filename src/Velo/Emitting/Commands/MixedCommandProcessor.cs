using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Velo.Emitting.Commands
{
    internal sealed class MixedCommandProcessor<TCommand> : ICommandProcessor<TCommand>, IAsyncCommandProcessor<TCommand>
        where TCommand : ICommand
    {
        private readonly ICommandHandler[] _handlers;

        public MixedCommandProcessor(List<ICommandHandler> handlers)
        {
            _handlers = handlers.ToArray();
        }

        public void Execute(TCommand command)
        {
            var context = new HandlerContext<TCommand>(command);
            var cancellationToken = CancellationToken.None;
            var handlers = _handlers;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < handlers.Length; i++)
            {
                var handler = handlers[i];
                switch (handler)
                {
                    case IAsyncCommandHandler<TCommand> asyncHandler:
                        asyncHandler.ExecuteAsync(context, cancellationToken).GetAwaiter().GetResult();
                        break;
                    case ICommandHandler<TCommand> syncHandler:
                        syncHandler.Execute(context);
                        break;
                }

                if (context.StopPropagation)
                {
                    break;
                }
            }
        }

        public async Task ExecuteAsync(TCommand command, CancellationToken cancellationToken)
        {
            var context = new HandlerContext<TCommand>(command);
            var handlers = _handlers;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < handlers.Length; i++)
            {
                var handler = handlers[i];
                switch (handler)
                {
                    case IAsyncCommandHandler<TCommand> asyncHandler:
                        await asyncHandler.ExecuteAsync(context, cancellationToken);
                        break;
                    case ICommandHandler<TCommand> syncHandler:
                        syncHandler.Execute(context);
                        break;
                }

                if (context.StopPropagation)
                {
                    break;
                }
            }
        }
    }
}