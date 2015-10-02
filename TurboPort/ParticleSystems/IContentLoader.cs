namespace TurboPort.ParticleSystems
{
    public interface IContentLoader
    {
        T Load<T>(string name);
    }
}