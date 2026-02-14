namespace TurboPort
{
    public interface ISoundHandler
    {
        void Bigexp(float volume = 1.0f);
        void Bingo(float volume = 1.0f);
        void Bullethit(float volume = 1.0f);
        void Checkpoint(float volume = 1.0f);
        void Fire(float volume = 1.0f);
        void FireMissile(float volume = 1.0f);
        void Shipcollide(float volume = 1.0f);
        void Splash(float volume = 1.0f);
        void Tingaling(float volume = 1.0f);
        void TochDown(float volume = 1.0f);
    }

    /// <summary>
    /// No-op sound handler for testing and headless scenarios.
    /// </summary>
    public class NullSoundHandler : ISoundHandler
    {
        public static readonly NullSoundHandler Instance = new();

        public void Bigexp(float volume = 1.0f) { }
        public void Bingo(float volume = 1.0f) { }
        public void Bullethit(float volume = 1.0f) { }
        public void Checkpoint(float volume = 1.0f) { }
        public void Fire(float volume = 1.0f) { }
        public void FireMissile(float volume = 1.0f) { }
        public void Shipcollide(float volume = 1.0f) { }
        public void Splash(float volume = 1.0f) { }
        public void Tingaling(float volume = 1.0f) { }
        public void TochDown(float volume = 1.0f) { }
    }
}
