using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TechCosmos.CommandSystem.Runtime
{
    public class CommandTarget<T> : MonoBehaviour
    {
        public Action<Vector3> OnMove;
        public Action<T> OnAttack;
        public Action OnStop;
        public void Move(Vector3 position)
        {
            OnMove(position);
        }
        public void Attack(T target)
        {
            OnAttack(target);
        }
        public void Stop()
        {
            OnStop();
        }
    }
}

