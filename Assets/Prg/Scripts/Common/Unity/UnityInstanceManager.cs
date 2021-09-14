using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Prg.Scripts.Common.Unity
{
    public class UnityInstanceManager : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _component;
        [SerializeField] private string _componentName;

        private static readonly Dictionary<string, MonoBehaviour> components = new Dictionary<string, MonoBehaviour>();

#if UNITY_EDITOR
        [SerializeField] private List<MonoBehaviour> componentList;

        private static void updateList()
        {
            var temp = components.Values.ToList();
            foreach (var instanceManager in FindObjectsOfType<UnityInstanceManager>())
            {
                instanceManager.updateList(temp);
            }
        }

        private void updateList(List<MonoBehaviour> newList)
        {
            componentList = newList;
        }
#endif
        private void Awake()
        {
            Debug.Log($"Awake {_componentName} : {_component.GetFullPath()}");
            if (components.ContainsKey(_componentName))
            {
                throw new UnityException("Duplicate component name: " + _componentName);
            }
            components.Add(_componentName, _component);
#if UNITY_EDITOR
            updateList();
#endif
        }

        private void OnDestroy()
        {
            Debug.Log($"OnDestroy {_componentName} : {_component.GetFullPath()}");
            components.Remove(_componentName);
#if UNITY_EDITOR
            updateList();
#endif
        }

        public static IReadOnlyCollection<MonoBehaviour> GetAll()
        {
            return components.Values.ToList().AsReadOnly();
        }

        public static IReadOnlyCollection<T> GetAll<T>(Type componentType) where T : MonoBehaviour
        {
            return components.Values
                .Where(x => x.GetType() == componentType)
                .Cast<T>()
                .ToList()
                .AsReadOnly();
        }

        public static bool TryGet<T>(string componentName, out T component) where T : MonoBehaviour
        {
            if (components.TryGetValue(componentName, out var result) && result is T typedResult)
            {
                component = typedResult;
                return true;
            }
            component = null;
            return false;
        }

        public static bool TryGet<T>(Type componentType, out T component) where T : MonoBehaviour
        {
            component = components.Values.FirstOrDefault(x => x.GetType() == componentType) as T;
            return component != null;
        }
    }
}