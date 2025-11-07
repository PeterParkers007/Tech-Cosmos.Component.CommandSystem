using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TechCosmos.CommandSystem.Runtime.Interfaces;
using TechCosmos.CommandSystem.Runtime.Enums;

namespace TechCosmos.CommandSystem.Runtime.Command
{
    public class CompositeCommand<T> : BaseCommand<T>
    {
        private List<ICommand> _commands = new List<ICommand>();
        private int _currentIndex = -1;
        private ICommand _currentCommand;

        public CompositeCommand(params ICommand[] commands)
        {
            _commands.AddRange(commands);
            // 监听所有子命令的状态变化
            foreach (var cmd in _commands)
            {
                cmd.OnStatusChanged += OnSubCommandStatusChanged;
            }
        }

        public void AddCommand(ICommand command)
        {
            _commands.Add(command);
            command.OnStatusChanged += OnSubCommandStatusChanged;
        }

        public override bool CanExecute() => base.CanExecute() && _commands.Any(c => c.CanExecute());

        public override void Execute()
        {
            if (!CanExecute())
            {
                OnExecuteFailed();
                return;
            }

            OnExecuteStart();
            ExecuteNextCommand();
        }

        private void ExecuteNextCommand()
        {
            _currentIndex++;

            if (_currentIndex < _commands.Count && !_isCancelled)
            {
                _currentCommand = _commands[_currentIndex];
                if (_currentCommand.CanExecute())
                {
                    _currentCommand.Execute();
                }
                else
                {
                    // 子命令无法执行，继续下一个
                    ExecuteNextCommand();
                }
            }
            else
            {
                // 所有命令执行完成或被取消
                if (!_isCancelled)
                    OnExecuteComplete();
                else
                    OnExecuteCancelled();
            }
        }

        private void OnSubCommandStatusChanged(CommandStatus status)
        {
            if (status == CommandStatus.Completed)
            {
                // 当前子命令完成，执行下一个
                ExecuteNextCommand();
            }
            else if (status == CommandStatus.Failed)
            {
                // 子命令失败，整个组合命令失败
                OnExecuteFailed();
            }
        }

        public override void Cancel()
        {
            base.Cancel();
            // 取消当前正在执行的子命令
            if (_currentCommand is ICancellableCommand cancellable)
            {
                cancellable.Cancel();
            }
        }
    }
}