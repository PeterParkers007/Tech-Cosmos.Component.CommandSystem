using System;
using System.Collections.Generic;
using UnityEngine;

namespace TechCosmos.CommandSystem.Runtime
{
    public class CommandTarget<T> : MonoBehaviour
    {
        private Dictionary<string, Delegate> _actionMap = new Dictionary<string, Delegate>();

        // 内置的基础动作（保持兼容性）
        public Action<Vector3> OnMove;
        public Action<T> OnAttack;
        public Action OnStop;

        public void Move(Vector3 position)
        {
            OnMove?.Invoke(position);
        }

        public void Attack(T target)
        {
            OnAttack?.Invoke(target);
        }

        public void Stop()
        {
            OnStop?.Invoke();
        }

        // 扩展方法：注册自定义动作
        public void RegisterAction<TAction>(string actionName, TAction action) where TAction : Delegate
        {
            _actionMap[actionName] = action;
        }

        // 扩展方法：执行自定义动作
        public void ExecuteAction(string actionName, params object[] parameters)
        {
            if (_actionMap.TryGetValue(actionName, out var action))
            {
                action?.DynamicInvoke(parameters);
            }
            else
            {
                Debug.LogWarning($"Action '{actionName}' not found on {name}");
            }
        }

        // 泛型版本，更安全
        public void ExecuteAction<TParam>(string actionName, TParam parameter)
        {
            if (_actionMap.TryGetValue(actionName, out var action))
            {
                if (action is Action<TParam> typedAction)
                {
                    typedAction.Invoke(parameter);
                }
                else
                {
                    Debug.LogError($"Action '{actionName}' type mismatch. Expected Action<{typeof(TParam).Name}>");
                }
            }
        }

        public void ExecuteAction(string actionName)
        {
            if (_actionMap.TryGetValue(actionName, out var action))
            {
                if (action is Action typedAction)
                {
                    typedAction.Invoke();
                }
                else
                {
                    Debug.LogError($"Action '{actionName}' type mismatch. Expected Action");
                }
            }
        }

        // 检查动作是否存在
        public bool HasAction(string actionName)
        {
            return _actionMap.ContainsKey(actionName);
        }

        // 移除动作
        public void UnregisterAction(string actionName)
        {
            _actionMap.Remove(actionName);
        }
    }
    public class CommandTarget : CommandTarget<MonoBehaviour>
    {
        
    }
}