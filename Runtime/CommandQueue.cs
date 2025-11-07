// 修改 Runtime/CommandQueue.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TechCosmos.CommandSystem.Runtime.Enums;
using TechCosmos.CommandSystem.Runtime.Interfaces;

namespace TechCosmos.CommandSystem.Runtime
{
    public class CommandQueue
    {
        private List<ICommand> _commandQueue = new List<ICommand>();
        private ICommand _currentCommand;

        public void Enqueue(ICommand command)
        {
            _commandQueue.Add(command);

            // 按优先级排序：高优先级在前
            _commandQueue = _commandQueue
                .OrderByDescending(c =>
                    (c as IPrioritizedCommand)?.Priority ?? CommandPriority.Normal)
                .ToList();
        }

        public void ExecuteNext()
        {
            // 清理已完成或失败的命令
            _commandQueue.RemoveAll(c =>
                c.Status == CommandStatus.Completed ||
                c.Status == CommandStatus.Failed ||
                c.Status == CommandStatus.Cancelled);

            if (_currentCommand == null && _commandQueue.Count > 0)
            {
                _currentCommand = _commandQueue[0];
                _commandQueue.RemoveAt(0);

                if (_currentCommand.CanExecute())
                {
                    // 使用 IStatusUpdatable 接口更新状态
                    if (_currentCommand is IStatusUpdatable updatable)
                    {
                        updatable.UpdateStatus(CommandStatus.Executing);
                    }

                    _currentCommand.Execute();

                    // 检查命令是否还在执行状态（说明它没有自行完成状态更新）
                    if (_currentCommand.Status == CommandStatus.Executing)
                    {
                        // 只记录警告，不强制修改状态
    Debug.LogWarning($"Command {_currentCommand.GetType().Name} is still in Executing status after Execute() call. " +
                    "The command should update its own status to Completed/Failed/Cancelled.");
                    }

                    _currentCommand = null;
                }
                else
                {
                    // 命令无法执行，标记为失败
                    if (_currentCommand is IStatusUpdatable updatable)
                    {
                        updatable.UpdateStatus(CommandStatus.Failed);
                    }
                    _currentCommand = null;
                }
            }
        }

        public void CancelCurrent()
        {
            if (_currentCommand is ICancellableCommand cancellable && cancellable.CanCancel)
            {
                cancellable.Cancel();
                // Cancel() 方法内部会更新状态，这里不需要再设置
            }
            _currentCommand = null;
        }

        public void Clear()
        {
            // 取消当前正在执行的命令
            CancelCurrent();

            // 清理队列中的所有命令
            foreach (var command in _commandQueue)
            {
                if (command is ICancellableCommand cancellable && cancellable.CanCancel)
                {
                    cancellable.Cancel();
                }
            }
            _commandQueue.Clear();
        }

        public bool HasCommands => _commandQueue.Count > 0 || _currentCommand != null;

        // 新增：获取队列信息（调试用）
        public string GetQueueInfo()
        {
            var currentInfo = _currentCommand != null ?
                $"Current: {_currentCommand.GetType().Name} ({_currentCommand.Status})" : "Current: None";

            var queueInfo = string.Join(", ", _commandQueue.Select(c =>
                $"{c.GetType().Name} ({(c as IPrioritizedCommand)?.Priority ?? CommandPriority.Normal})"));

            return $"{currentInfo} | Queue: [{queueInfo}]";
        }
    }
}