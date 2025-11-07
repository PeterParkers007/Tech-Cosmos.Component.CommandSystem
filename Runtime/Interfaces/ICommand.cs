using System.Collections;
using System.Collections.Generic;
using TechCosmos.CommandSystem.Runtime.Enums;
using UnityEngine;
namespace TechCosmos.CommandSystem.Runtime.Interfaces
{
    public interface ICommand
    {
        bool CanExecute();
        void Execute();
        CommandStatus Status { get; }

        // 生命周期事件
        System.Action<CommandStatus> OnStatusChanged { get; set; }
    }
}

