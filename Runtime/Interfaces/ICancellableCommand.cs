using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechCosmos.CommandSystem.Runtime.Interfaces
{
    public interface ICancellableCommand : ICommand
    {
        void Cancel();
        bool CanCancel { get; }
    }
}
