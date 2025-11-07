// 修改 Runtime/CommandExtensions.cs
using TechCosmos.CommandSystem.Runtime.Command;
using TechCosmos.CommandSystem.Runtime.Enums;
using TechCosmos.CommandSystem.Runtime.Interfaces;
using System;
namespace TechCosmos.CommandSystem.Runtime
{
    public static class CommandExtensions
    {
        // 链式执行多个命令
        public static CompositeCommand<T> Then<T>(this ICommand first, ICommand next)
        {
            if (first is CompositeCommand<T> composite)
            {
                composite.AddCommand(next);
                return composite;
            }
            else
            {
                return new CompositeCommand<T>(first, next);
            }
        }

        // 设置命令优先级
        public static TCommand WithPriority<TCommand>(this TCommand command, CommandPriority priority)
            where TCommand : IPrioritizedCommand
        {
            command.Priority = priority;
            return command;
        }

        // 添加条件执行
        public static ConditionalCommand<T> When<T>(this ICommand command, Func<bool> condition)
        {
            return new ConditionalCommand<T>(command, condition);
        }
    }
}