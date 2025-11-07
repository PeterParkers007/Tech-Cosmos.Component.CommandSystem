using System;
using TechCosmos.CommandSystem.Runtime.Enums;
using TechCosmos.CommandSystem.Runtime.Interfaces;
using UnityEngine;

namespace TechCosmos.CommandSystem.Runtime.Command
{
    public class ConditionalCommand<T> : BaseCommand<T>
    {
        private ICommand _wrappedCommand;
        private Func<bool> _condition;

        public ConditionalCommand(ICommand command, Func<bool> condition)
        {
            _wrappedCommand = command;
            _condition = condition;

            // 监听包装命令的状态变化
            _wrappedCommand.OnStatusChanged += OnWrappedCommandStatusChanged;
        }

        public override bool CanExecute() => base.CanExecute() && _condition() && _wrappedCommand.CanExecute();

        public override void Execute()
        {
            if (!CanExecute())
            {
                OnExecuteFailed();
                return;
            }

            OnExecuteStart();
            _wrappedCommand.Execute();
        }

        private void OnWrappedCommandStatusChanged(CommandStatus status)
        {
            // 同步包装命令的状态
            switch (status)
            {
                case CommandStatus.Completed:
                    OnExecuteComplete();
                    break;
                case CommandStatus.Failed:
                    OnExecuteFailed();
                    break;
                case CommandStatus.Cancelled:
                    OnExecuteCancelled();
                    break;
                    // Executing 状态已经在 Execute() 中设置了，不需要重复设置
            }
        }

        public override void Cancel()
        {
            base.Cancel();
            // 取消包装的命令
            if (_wrappedCommand is ICancellableCommand cancellable)
            {
                cancellable.Cancel();
            }
        }
    }
}