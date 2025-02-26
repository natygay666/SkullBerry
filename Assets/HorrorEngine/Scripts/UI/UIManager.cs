using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HorrorEngine
{
    public struct UIStackedAction
    {
        public UnityAction Action;
        public bool StopProcessingActions;
    }

    public class UIManager : SingletonBehaviour<UIManager>
    {
        private Dictionary<Type, Component> m_CachedUI = new Dictionary<Type, Component>();
        private Stack<UIStackedAction> m_ActionStack = new Stack<UIStackedAction>();

        public static T Get<T>() where T:Component
        {
            UIManager ui = Instance;
            Type type = typeof(T);
            if (!ui.m_CachedUI.ContainsKey(type))
            {
                var component = ui.GetComponentInChildren<T>(true);
                if (component)
                {
                    ui.m_CachedUI.Add(type, component);
                }
            }

            ui.m_CachedUI.TryGetValue(type, out Component value);
            Debug.Assert(value, $"UIManager couldn't find a component of type {type.Name}");

            return value as T;
        } 

        public static void PushAction(UIStackedAction item)
        {
            UIManager ui = Instance;
            ui.m_ActionStack.Push(item);
        }

        public static bool PopAction()
        {
            UIManager ui = Instance;

            int processed = 0;
            UIStackedAction item;
            do
            {
                if (ui.m_ActionStack.Count == 0)
                    return processed > 0;

                item = ui.m_ActionStack.Pop();
                item.Action?.Invoke();
                ++processed;
            } while (!item.StopProcessingActions);

            return true;
        }
    }
}