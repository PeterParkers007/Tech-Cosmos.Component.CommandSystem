// дк Runtime/BaseCommand.cs
using System;
using TechCosmos.CommandSystem.Runtime.Enums;
using TechCosmos.CommandSystem.Runtime.Interfaces;

namespace TechCosmos.CommandSystem.Runtime
{
    public abstract class BaseCommand : ICommand, IStatusUpdatable
    {
        private CommandStatus _status = CommandStatus.Pending;

        public CommandStatus Status => _status;
        public Action<CommandStatus> OnStatusChanged { get; set; }

        public virtual bool CanExecute() => true;
        public abstract void Execute();

        public void UpdateStatus(CommandStatus newStatus)
        {
            if (_status != newStatus)
            {
                _status = newStatus;
                OnStatusChanged?.Invoke(_status);
            }
        }

        protected void OnExecuteStart() => UpdateStatus(CommandStatus.Executing);
        protected void OnExecuteComplete() => UpdateStatus(CommandStatus.Completed);
        protected void OnExecuteFailed() => UpdateStatus(CommandStatus.Failed);
        protected void OnExecuteCancelled() => UpdateStatus(CommandStatus.Cancelled);
    }

    public abstract class BaseCommand<T> : BaseCommand, ICancellableCommand, IPrioritizedCommand
    {
        protected bool _isCancelled = false;

        public virtual bool CanCancel => true;
        public virtual CommandPriority Priority { get; set; } = CommandPriority.Normal;

        public virtual void Cancel()
        {
            if (CanCancel)
            {
                _isCancelled = true;
                UpdateStatus(CommandStatus.Cancelled);
            }
        }

        public override bool CanExecute() => !_isCancelled && base.CanExecute();
    }
}