using TechCosmos.CommandSystem.Runtime.Interfaces;
using System.Collections.Generic;
using UnityEngine;
namespace TechCosmos.CommandSystem.Runtime
{
    public abstract class CommandManager<T> : MonoBehaviour
    {
        private static CommandManager<T> instance;
        public static CommandManager<T> Instance
        {
            get { return instance; }
        }

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
                // 跨场景不销毁
                DontDestroyOnLoad(gameObject);
            }
        }
        public virtual void ExecuteCommand(CommandTarget<T> unit, ICommand command)
        {
            if (!_unitCommandQueues.ContainsKey(unit))
            {
                _unitCommandQueues[unit] = new CommandQueue();
            }

            // 简单实现：立即执行，不清除队列
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

