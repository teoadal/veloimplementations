namespace Velo.Emitting.Commands
{
    internal interface ICommandProcessor
    {
    }
    
    internal interface ICommandProcessor<in TCommand> : ICommandProcessor
        where TCommand : ICommand
    {
        void Execute(TCommand command);
    }
}