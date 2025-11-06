using System;
using TechCosmos.CommandSystem.Runtime.Interfaces;
using TechCosmos.CommandSystem.Runtime;
using UnityEngine;
namespace TechCosmos.CommandSystem.Runtime.Command
{
    public class MoveCommand<T> : ICommand
    {
        private CommandTarget<T> _commandTarget;
        private Vector3 _targetPosition;
        public MoveCommand(CommandTarget<T> commandTarget, Vector3 targetPosition)
        {
            _commandTarget = commandTarget;
            _targetPosition = targetPosition;
        }
        public void Execute()
        {
            if (CanExecute())
            {
                _commandTarget.Move(_targetPosition);
            }
        }

        public bool CanExecute() => _commandTarget != null;
    }
}

