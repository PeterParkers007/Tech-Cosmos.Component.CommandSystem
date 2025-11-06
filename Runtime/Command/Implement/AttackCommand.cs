using TechCosmos.CommandSystem.Runtime.Interfaces;
namespace TechCosmos.CommandSystem.Runtime.Command
{
    public class AttackCommand<T> : ICommand
    {
        private CommandTarget<T> _attacker;
        private T _target;
        public AttackCommand(CommandTarget<T> attacker, T target)
        {
            _attacker = attacker;
            _target = target;
        }
        public bool CanExecute() => _attacker != null && _target != null;

        public void Execute()
        {
            if (CanExecute())
                _attacker.Attack(_target);
        }
    }
}

