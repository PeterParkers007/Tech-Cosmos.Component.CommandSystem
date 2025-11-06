using System.Collections.Generic;
using TechCosmos.CommandSystem.Runtime.Interfaces;
namespace TechCosmos.CommandSystem.Runtime
{
    public class CommandQueue
    {
        private Queue<ICommand> _commandQueue = new Queue<ICommand>();
        private ICommand _currentCommand;

        public void Enqueue(ICommand command)
        {
            _commandQueue.Enqueue(command);
        }

        public void ExecuteNext()
        {
            if (_currentCommand == null && _commandQueue.Count > 0)
            {
                _currentCommand = _commandQueue.Dequeue();
                if (_currentCommand.CanExecute())
                {
                    _currentCommand.Execute();
                    _currentCommand = null; // 简单实现：立即完成
                }
            }
        }

        public void Clear()
        {
            _commandQueue.Clear();
            _currentCommand = null;
        }

        public bool HasCommands => _commandQueue.Count > 0 || _currentCommand != null;
    }
}
