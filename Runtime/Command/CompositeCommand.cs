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
            // 安全检查：确保在有效范围内且没有被取消
            if (_currentIndex < 0 || _currentIndex >= _commands.Count || _isCancelled)
                return;
            
            // 验证当前命令确实是触发状态变化的命令
            var triggeringCommand = _commands[_currentIndex];
            if (triggeringCommand.Status != status)
                return;

            switch (status)
            {
                case CommandStatus.Completed:
                    // 当前子命令完成，执行下一个
                    ExecuteNextCommand();
                    break;
                    
                case CommandStatus.Failed:
                    // 子命令失败，整个组合命令失败
                    OnExecuteFailed();
                    break;
                    
                case CommandStatus.Cancelled:
                    // 子命令被取消，整个组合命令也被取消
                    OnExecuteCancelled();
                    break;
                    
                case CommandStatus.Executing:
                    // 子命令开始执行，不需要特殊处理
                    break;
                    
                case CommandStatus.Pending:
                    // 不应该发生，但忽略即可
                    break;
                    
                default:
                    Debug.LogWarning($"Unexpected command status: {status}");
                    break;
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