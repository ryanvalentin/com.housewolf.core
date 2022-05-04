using System;
using System.Collections.Generic;

namespace Housewolf.EntitySystem
{
    public class BaseEntityManager<T> where T : IEntity
    {
        public List<T> Entities = new List<T>();

        public int EntityCount => Entities.Count;

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
        }

        protected virtual void DisposeAll()
        {

        }

        protected void ForEachEntity(Action<T, int> callback)
        {
            for (int i = 0; i < EntityCount; i++)
            {
                callback.Invoke(Entities[i], i);
            }
        }
    }
}
