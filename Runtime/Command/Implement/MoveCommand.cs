using System;
using UnityEngine;
using TechCosmos.CommandSystem.Runtime.Interfaces;
using TechCosmos.CommandSystem.Runtime.Enums;

namespace TechCosmos.CommandSystem.Runtime.Command
{
    public class MoveCommand<T> : BaseCommand<T>
    {
        private CommandTarget<T> _commandTarget;
        private Vector3 _targetPosition;

        public MoveCommand(CommandTarget<T> commandTarget, Vector3 targetPosition, CommandPriority priority = CommandPriority.Normal)
        {
            _commandTarget = commandTarget;
            _targetPosition = targetPosition;
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
                    _commandTarget.Move(_targetPosition);
                    OnExecuteComplete();
                }
                else
                {
                    OnExecuteCancelled();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Move command failed: {ex.Message}");
                OnExecuteFailed();
            }
        }
    }
}