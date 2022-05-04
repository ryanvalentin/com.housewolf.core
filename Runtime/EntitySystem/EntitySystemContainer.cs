using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Housewolf.EntitySystem
{
    /// <summary>
    /// Container for all the entity managers which run in the game.
    /// </summary>
    public class EntitySystemContainer : MonoBehaviour
    {
        private static EntitySystemContainer _current;
        public static EntitySystemContainer Current
        {
            get
            {
                return _current;
            }
        }

        private readonly Dictionary<string, IEntityManager> _managers = new Dictionary<string, IEntityManager>();

        private readonly Dictionary<string, bool> _managerRunQueue = new Dictionary<string, bool>();

        public Dictionary<string, IEntityManager> Managers => _managers;

        public T GetManager<T>() where T : IEntityManager, new()
        {
            string key = GetKey<T>();
            if (!_managers.ContainsKey(key))
            {
                var newManager = new T();
                _managers.Add(key, newManager);
                _managerRunQueue.Add(key, false);
                newManager.HandleInit();
            }

            return (T)(_managers[key]);
        }

        //
        // Convenience methods

        private string GetKey<T>()where T : IEntityManager => typeof(T).FullName;

        private string GetKey(IEntityManager manager) => manager.GetType().FullName;

        private bool HasManagerRun(IEntityManager manager) => manager != null && _managerRunQueue.TryGetValue(GetKey(manager), out bool hasRun) && hasRun;

        private void ResetQueue()
        {
            foreach (var queueKey in _managerRunQueue.Keys.ToArray())
            {
                _managerRunQueue[queueKey] = false;
            }
        }

        private void InvokeManagerMethods(Action<IEntityManager> callback)
        {
            ResetQueue();

            foreach (var managerKV in _managers)
            {
                IEntityManager manager = managerKV.Value;

                // Skip if we've run this, like in a dependency loop.
                if (HasManagerRun(manager))
                    continue;

                IEntityManager dependency = manager.Dependency;
                while (dependency != null)
                {
                    if (!HasManagerRun(dependency))
                    {
                        // Run the dependent method.
                        callback(dependency);

                        // Mark this manager as run.
                        _managerRunQueue[GetKey(dependency)] = true;

                        // Recurse upward until complete.
                        dependency = dependency.Dependency;
                    }
                    else
                    {
                        dependency = null;
                    }
                }

                callback(manager);
            }
        }

        //
        // Lifecycle Methods

        private void Awake()
        {
            if (_current)
                Debug.LogWarning("Two Entity System Containers detected, only the last one will be used.");

            _current = this;
        }

        private void Start()
        {
            InvokeManagerMethods(manager => manager.HandleInit());
        }

        private void FixedUpdate()
        {
            InvokeManagerMethods(manager => manager.HandlePhysicsUpdate());
        }

        private void Update()
        {
            InvokeManagerMethods(manager => manager.HandleUpdate());
        }

        private void LateUpdate()
        {
            InvokeManagerMethods(manager => manager.HandleLateUpdate());
        }

        private void OnDestroy()
        {
            InvokeManagerMethods(manager => manager.HandleDestroy());

            _current = null;
        }
    }
}
