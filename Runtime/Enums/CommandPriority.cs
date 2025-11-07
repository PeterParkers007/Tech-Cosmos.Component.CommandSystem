using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechCosmos.CommandSystem.Runtime.Enums
{
    public enum CommandPriority
    {
        Low = 0,      // 低优先级：采集、建造等
        Normal = 1,   // 普通优先级：移动、工作
        High = 2,     // 高优先级：战斗、紧急躲避
        Immediate = 3 // 立即执行：打断当前命令
    }
}
