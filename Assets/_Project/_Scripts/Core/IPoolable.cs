namespace Shield_Shot.Core
{
    public interface IPoolable
    {
        void OnSpawnedFromPool();
        void OnReturnedToPool();
    }
}
