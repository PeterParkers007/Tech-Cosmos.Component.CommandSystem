using System;
using UnityEngine;
using TechCosmos.CommandSystem.Runtime.Interfaces;
using TechCosmos.CommandSystem.Runtime.Enums;

namespace TechCosmos.CommandSystem.Runtime.Command
{
    public class AttackCommand<T> : BaseCommand<T>
    {
        private CommandTarget<T> _attacker;
        private T _target;

        public AttackCommand(CommandTarget<T> attacker, T target, CommandPriority priority = CommandPriority.Normal)
        {
            _attacker = attacker;
            _target = target;
            Priority = priority;
        }

        public override bool CanExecute() => base.CanExecute() && _attacker != null && _target != null;

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
                    _attacker.Attack(_target);
                    OnExecuteComplete();
                }
                else
                {
                    OnExecuteCancelled();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Attack command failed: {ex.Message}");
                OnExecuteFailed();
            }
        }
    }
}