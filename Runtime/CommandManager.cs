using TechCosmos.CommandSystem.Runtime.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace TechCosmos.CommandSystem.Runtime
{
    public abstract class CommandManager<T> : MonoBehaviour
    {
        private static CommandManager<T> instance;
        public static CommandManager<T> Instance => instance;

        private Dictionary<CommandTarget<T>, CommandQueue> _unitCommandQueues = new Dictionary<CommandTarget<T>, CommandQueue>();

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

        public virtual void ExecuteCommand(CommandTarget<T> unit, ICommand command)
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

        public virtual void EnqueueCommand(CommandTarget<T> unit, ICommand command)
        {
            if (!_unitCommandQueues.ContainsKey(unit))
            {
                _unitCommandQueues[unit] = new CommandQueue();
            }

            _unitCommandQueues[unit].Enqueue(command);
        }

        // 新增方法
        public virtual void ExecuteImmediateCommand(CommandTarget<T> unit, ICommand command)
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

        public virtual void CancelAllCommands(CommandTarget<T> unit)
        {
            if (_unitCommandQueues.ContainsKey(unit))
            {
                _unitCommandQueues[unit].Clear();
            }
        }

        public virtual void CancelCurrentCommand(CommandTarget<T> unit)
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

        public virtual void ClearUnitCommands(CommandTarget<T> unit)
        {
            if (_unitCommandQueues.ContainsKey(unit))
            {
                _unitCommandQueues[unit].Clear();
            }
        }
    }
}