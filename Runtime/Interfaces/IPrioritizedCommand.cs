using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TechCosmos.CommandSystem.Runtime.Enums;
namespace TechCosmos.CommandSystem.Runtime.Interfaces
{
    public interface IPrioritizedCommand
    {
        CommandPriority Priority { get; set; }
    }
}
