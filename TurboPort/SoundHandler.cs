using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace TurboPort
{

    public class SoundHandler : ISoundHandler
    {
        public static ISoundHandler Current { get; set; } = NullSoundHandler.Instance;

        private static GameTime gameTime;
        private static SoundEffect lastPlayedSoundEffect;
        private static TimeSpan lastPlayedSoundEffectGameTime;

        private static SoundEffect explosion2;
        private static SoundEffect bigexp;
        private static SoundEffect bingo;
        private static SoundEffect bullethit;
        private static SoundEffect checkpoint;
        private static SoundEffect fire;
        private static SoundEffect fireMissile;
        private static SoundEffect shipcollide;
        private static SoundEffect shipcollide2;
        private static SoundEffect splash;
        private static SoundEffect tingaling;
        private static SoundEffect touchdown;

        static public void SetGameTime(GameTime gameTime)
        {
            SoundHandler.gameTime = gameTime;
        }

        static public void Initialize(ContentManager content)
        {
#if DONT_KNOW_HOW_TO_INSTALL_SOUND
            return;
#endif
            explosion2 = content.Load<SoundEffect>(@"sound/Explosion2");
            bigexp = content.Load<SoundEffect>(@"sound/bigexp");
            bingo = content.Load<SoundEffect>(@"sound/bingo");
            bullethit = content.Load<SoundEffect>(@"sound/bullethit");
            checkpoint = content.Load<SoundEffect>(@"sound/checkpoint");
            fire = content.Load<SoundEffect>(@"sound/fire");
            fireMissile = content.Load<SoundEffect>(@"sound/firemissle");
            shipcollide = content.Load<SoundEffect>(@"sound/shipcollide");
            shipcollide2 = content.Load<SoundEffect>(@"sound/shipcollide2");
            splash = content.Load<SoundEffect>(@"sound/splash");
            tingaling = content.Load<SoundEffect>(@"sound/tingaling");
            touchdown = content.Load<SoundEffect>(@"sound/touchdown");
        }

        static private void Play(SoundEffect soundEffect, float volume)
        {
#if DONT_KNOW_HOW_TO_INSTALL_SOUND
			return;
#endif
            if (soundEffect == null)
                return;

            // too basic protection against too many Play calls which will cause crashes
            if (soundEffect == lastPlayedSoundEffect)
            {
                if (gameTime.TotalGameTime - lastPlayedSoundEffectGameTime < TimeSpan.FromSeconds(0.2))
                    return;
            }

            lastPlayedSoundEffect = soundEffect;
            lastPlayedSoundEffectGameTime = gameTime.TotalGameTime;
            soundEffect.Play(volume, 0, 0);
        }

        // Instance methods (ISoundHandler implementation)
        void ISoundHandler.Bigexp(float volume)         { Play(bigexp, volume); }
        void ISoundHandler.Bingo(float volume)          { Play(bingo, volume); }
        void ISoundHandler.Bullethit(float volume)      { Play(bullethit, volume); }
        void ISoundHandler.Checkpoint(float volume)     { Play(checkpoint, volume); }
        void ISoundHandler.Fire(float volume)           { Play(fire, volume); }
        void ISoundHandler.FireMissile(float volume)     { Play(fireMissile, volume); }
        void ISoundHandler.Shipcollide(float volume)    { Play(shipcollide, volume); }
        void ISoundHandler.Splash(float volume)         { Play(splash, volume);  }
        void ISoundHandler.Tingaling(float volume)      { Play(tingaling, volume); }
        void ISoundHandler.TochDown(float volume)       { Play(touchdown, volume); }

        // Static convenience methods that delegate to Current
        static public void Bigexp(float volume = 1.0f)         { Current.Bigexp(volume); }
        static public void Bingo(float volume = 1.0f)          { Current.Bingo(volume); }
        static public void Bullethit(float volume = 1.0f)      { Current.Bullethit(volume); }
        static public void Checkpoint(float volume = 1.0f)     { Current.Checkpoint(volume); }
        static public void Fire(float volume = 1.0f)           { Current.Fire(volume); }
        static public void FireMissile(float volume = 1.0f)     { Current.FireMissile(volume); }
        static public void Shipcollide(float volume = 1.0f)    { Current.Shipcollide(volume); }
        static public void Splash(float volume = 1.0f)         { Current.Splash(volume);  }
        static public void Tingaling(float volume = 1.0f)      { Current.Tingaling(volume); }
        static public void TochDown(float volume = 1.0f)       { Current.TochDown(volume); }
    }
}
