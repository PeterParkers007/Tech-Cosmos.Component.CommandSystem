using TechCosmos.CommandSystem.Runtime.Interfaces;
namespace TechCosmos.CommandSystem.Runtime.Command
{
    public class StopCommand<T> : ICommand
    {
        public CommandTarget<T> _commandTarget;

        public StopCommand(CommandTarget<T> commandTarget)
        {
            _commandTarget = commandTarget;
        }

        public bool CanExecute()
        {
            return _commandTarget != null;
        }

        public void Execute()
        {
            if (CanExecute())
            {
                _commandTarget.Stop();
            }
        }
    }
}

