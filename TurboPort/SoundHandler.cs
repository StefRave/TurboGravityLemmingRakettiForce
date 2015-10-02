using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace TurboPort
{

    public class SoundHandler
    {
        private static SoundEffect explosion2;
        private static SoundEffect bigexp;
        private static SoundEffect bingo;
        private static SoundEffect bullethit;
        private static SoundEffect checkpoint;
        private static SoundEffect fire;
        private static SoundEffect firemissle;
        private static SoundEffect shipcollide;
        private static SoundEffect shipcollide2;
        private static SoundEffect splash;
        private static SoundEffect tingaling;
        private static SoundEffect touchdown;

        // Audio objects

        static public void Initialize(ContentManager content)
        {
            explosion2 = content.Load<SoundEffect>(@"sound/Explosion2");
            bigexp = content.Load<SoundEffect>(@"sound/bigexp");
            bingo = content.Load<SoundEffect>(@"sound/bingo");
            bullethit = content.Load<SoundEffect>(@"sound/bullethit");
            checkpoint = content.Load<SoundEffect>(@"sound/checkpoint");
            fire = content.Load<SoundEffect>(@"sound/fire");
            firemissle = content.Load<SoundEffect>(@"sound/firemissle");
            shipcollide = content.Load<SoundEffect>(@"sound/shipcollide");
            shipcollide2 = content.Load<SoundEffect>(@"sound/shipcollide2");
            splash = content.Load<SoundEffect>(@"sound/splash");
            tingaling = content.Load<SoundEffect>(@"sound/tingaling");
            touchdown = content.Load<SoundEffect>(@"sound/touchdown");
        }

        static public void Bigexp(float volume = 1.0f)         { bigexp.Play(volume, 0, 0); }
        static public void Bingo(float volume = 1.0f)          { bingo.Play(volume, 0, 0); }
        static public void Bullethit(float volume = 1.0f)      { bullethit.Play(volume, 0, 0); }
        static public void Checkpoint(float volume = 1.0f)     { checkpoint.Play(volume, 0, 0); }
        static public void Fire(float volume = 1.0f)           { fire.Play(volume, 0, 0); }
        static public void Firemissle(float volume = 1.0f)     { firemissle.Play(volume, 0, 0); }
        static public void Shipcollide(float volume = 1.0f)    { shipcollide.Play(volume, 0, 0); }
        static public void Splash(float volume = 1.0f)         { splash.Play(volume, 0, 0);  }
        static public void Tingaling(float volume = 1.0f)      { tingaling.Play(volume, 0, 0); }
        static public void TochDown(float volume = 1.0f)       { touchdown.Play(volume, 0, 0); }
    }
}
