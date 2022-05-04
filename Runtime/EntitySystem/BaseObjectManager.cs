using System;
using System.Collections.Generic;
using UnityEngine;

namespace Housewolf.EntitySystem
{
    public class BaseObjectManager<T> : ScriptableObject, IEntityManager where T : IEntity
    {
        public List<T> Entities = new List<T>();

        public int EntityCount => Entities.Count;

        public virtual IEntityManager Dependency => null;

        public virtual int Register(T entity)
        {
            int newIndex = Entities.Count;
            Entities.Add(entity);

            return newIndex;
        }

        public virtual void HandleInit()
        {

        }

        public virtual void HandlePhysicsUpdate()
        {

        }

        public virtual void HandleUpdate()
        {

        }

        public virtual void HandleLateUpdate()
        {

        }

        public virtual void HandleDestroy()
        {
            DisposeAll();
            Entities.Clear(); // Remove all entities so it isn't saved with an empty array.
        }

        protected virtual void DisposeAll()
        {

        }

        protected void ForEachEntity(Action<T, int> callback)
        {
            for (int i = 0; i < EntityCount; i++)
            {
                if (Entities[i] == null)
                    continue;

                callback.Invoke(Entities[i], i);
            }
        }
    }
}
