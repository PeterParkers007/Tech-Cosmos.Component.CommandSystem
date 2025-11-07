// ÔÚ Runtime/ ÖÐÌí¼Ó AbstractCommand.cs
using System;
using TechCosmos.CommandSystem.Runtime.Enums;
using TechCosmos.CommandSystem.Runtime.Interfaces;

namespace TechCosmos.CommandSystem.Runtime
{
    public abstract class AbstractCommand<T> : ICommand, ICancellableCommand, IPrioritizedCommand
    {
        public CommandStatus Status { get; set; } = CommandStatus.Pending;
        public Action<CommandStatus> OnStatusChanged { get; set; }
        public virtual bool CanCancel => true;
        public virtual CommandPriority Priority { get; set; } = CommandPriority.Normal;

        protected bool _isCancelled = false;

        public virtual void Cancel()
        {
            if (!CanCancel) return;

            _isCancelled = true;
            UpdateStatus(CommandStatus.Cancelled);
        }

        public abstract bool CanExecute();
        public abstract void Execute();

        protected void UpdateStatus(CommandStatus newStatus)
        {
            Status = newStatus;
            OnStatusChanged?.Invoke(newStatus);
        }

        protected virtual void OnExecuteStart()
        {
            UpdateStatus(CommandStatus.Executing);
        }

        protected virtual void OnExecuteComplete()
        {
            if (!_isCancelled)
                UpdateStatus(CommandStatus.Completed);
            else
                UpdateStatus(CommandStatus.Cancelled);
        }

        protected virtual void OnExecuteFailed()
        {
            UpdateStatus(CommandStatus.Failed);
        }
    }
}