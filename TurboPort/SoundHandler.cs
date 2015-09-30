using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Audio;



namespace tglrf
{

    public class SoundHandler
    {

        // Audio objects
        static AudioEngine engine;
        static SoundBank soundBank;
        static WaveBank waveBank;

		static private void Play(string name)
        {
			// NOTE (san): Sound engine not working so disabling sound playback for now
            //soundBank.PlayCue(name);
        }

        static public void Initialize()
        {
            // Initialize audio objects.
            engine = new AudioEngine(@"Content/sound/tglrf.xgs");
            soundBank = new SoundBank(engine, @"Content/sound/Sound Bank.xsb");
            waveBank = new WaveBank(engine, @"Content/sound/Wave Bank.xwb");
        }

        static public void Bigexp()         { Play("bigexp"); }
        //static public void Bingo()          { Play(""); }
        static public void Bullethit()      { Play("bullethit"); }
        static public void Checkpoint()     { Play("checkpoint"); }
        static public void Firemissle()     { Play("firemissle"); }
        static public void Shipcollide()    { Play("shipcollide"); }
        static public void BigexpBuffer()   { Play("Explosion2"); }
        static public void Splash()         { Play("splash"); }
        static public void Tingaling()      { Play("tingaling"); }
        static public void TochDown()       { Play("touchdown"); }
        static public void Fire()           { Play("fire"); }
    }
}
