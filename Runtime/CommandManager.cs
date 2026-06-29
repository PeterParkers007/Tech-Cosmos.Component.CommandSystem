using TechCosmos.CommandSystem.Runtime.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace TechCosmos.CommandSystem.Runtime
{
    public class CommandManager : MonoBehaviour
    {
        private static CommandManager instance;
        public static CommandManager Instance => instance;

        private Dictionary<ICommandTarget, CommandQueue> _unitCommandQueues = new Dictionary<ICommandTarget, CommandQueue>();

        protected virtual void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public virtual void ExecuteCommand(ICommandTarget unit, ICommand command)
        {
            if (!_unitCommandQueues.ContainsKey(unit))
            {
                _unitCommandQueues[unit] = new CommandQueue();
            }

            if (command.CanExecute())
            {
                command.Execute();
            }
        }

        public virtual void EnqueueCommand(ICommandTarget unit, ICommand command)
        {
            if (!_unitCommandQueues.ContainsKey(unit))
            {
                _unitCommandQueues[unit] = new CommandQueue();
            }

            _unitCommandQueues[unit].Enqueue(command);
        }

        // ��������
        public virtual void ExecuteImmediateCommand(ICommandTarget unit, ICommand command)
        {
            if (!_unitCommandQueues.ContainsKey(unit))
            {
                _unitCommandQueues[unit] = new CommandQueue();
            }

            _unitCommandQueues[unit].Clear();

            if (command.CanExecute())
            {
                command.Execute();
            }
        }

        public virtual void CancelAllCommands(ICommandTarget unit)
        {
            if (_unitCommandQueues.ContainsKey(unit))
            {
                _unitCommandQueues[unit].Clear();
            }
        }

        public virtual void CancelCurrentCommand(ICommandTarget unit)
        {
            if (_unitCommandQueues.ContainsKey(unit))
            {
                _unitCommandQueues[unit].CancelCurrent();
            }
        }

        public virtual void ObserverUpdate()
        {
            foreach (var queue in _unitCommandQueues.Values)
            {
                queue.ExecuteNext();
            }
        }

        public virtual void ClearUnitCommands(ICommandTarget unit)
        {
            if (_unitCommandQueues.ContainsKey(unit))
            {
                _unitCommandQueues[unit].Clear();
            }
        }
    }
}