using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechCosmos.CommandSystem.Runtime.Enums
{
    public enum CommandStatus
    {
        Pending,     // 等待执行
        Executing,   // 执行中
        Completed,   // 执行完成
        Failed,      // 执行失败
        Cancelled    // 被取消
    }
}
