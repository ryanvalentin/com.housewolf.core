namespace Housewolf.EntitySystem
{
    public interface IEntityManager
    {
        int EntityCount { get; }

        IEntityManager Dependency { get; }

        void HandleInit();

        void HandlePhysicsUpdate();

        void HandleUpdate();

        void HandleLateUpdate();

        void HandleDestroy();
    }
}
