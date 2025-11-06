using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TechCosmos.CommandSystem.Runtime.Interfaces
{
    public interface ICommand
    {
        bool CanExecute();
        void Execute();
    }
}

