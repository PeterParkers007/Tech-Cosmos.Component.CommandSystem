using System;
using UnityEngine;
using TechCosmos.CommandSystem.Runtime.Interfaces;
using TechCosmos.CommandSystem.Runtime.Enums;

namespace TechCosmos.CommandSystem.Runtime.Command
{
    public class StopCommand<T> : BaseCommand<T>
    {
        private CommandTarget<T> _commandTarget;

        public StopCommand(CommandTarget<T> commandTarget, CommandPriority priority = CommandPriority.High)
        {
            _commandTarget = commandTarget;
            Priority = priority;
        }

        public override bool CanExecute() => base.CanExecute() && _commandTarget != null;

        public override void Execute()
        {
            if (!CanExecute())
            {
                OnExecuteFailed();
                return;
            }

            OnExecuteStart();

            try
            {
                if (!_isCancelled)
                {
                    _commandTarget.Stop();
                    OnExecuteComplete();
                }
                else
                {
                    OnExecuteCancelled();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Stop command failed: {ex.Message}");
                OnExecuteFailed();
            }
        }
    }
}