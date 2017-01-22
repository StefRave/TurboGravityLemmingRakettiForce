#region Using Statements

using System;
using System.Diagnostics;

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;

#elif __IOS__
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif
#endregion

namespace TurboPort
{
#if __IOS__
	[Register("AppDelegate")]
	class Program : UIApplicationDelegate
	
#else
    static class Program
#endif
    {
        private static Game1 game;

        internal static void RunGame(GameMode gameMode = 0)
        {
            game = new Game1(gameMode);
            try
            {
                game.Run();
            }
            catch (NullReferenceException)
            {
                // This is caused by Dispose(true); 
                // But is is added as a workaround to 
                // https://github.com/mono/MonoGame/issues/3749
                // [3.3] OpenTK crashes if you exit on windows without calling dispose on textures #3749
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
#if !MONOMAC && !__IOS__
        [STAThread]
#endif
        static void Main(string[] args)
        {
#if MONOMAC
			NSApplication.Init ();

			using (var p = new NSAutoreleasePool ()) {
				NSApplication.SharedApplication.Delegate = new AppDelegate();
				NSApplication.Main(args);
			}
#elif __IOS__
			UIApplication.Main(args, null, "AppDelegate");
#else
            GameMode gameMode = GameMode.Multiplayer;
            if (args.Length >= 1)
                Enum.TryParse(args[0], true, out gameMode);
            RunGame(gameMode);
#endif
        }

#if __IOS__
		public override void FinishedLaunching(UIApplication app)
		{
			RunGame();
		}
#endif
    }

#if MONOMAC
	class AppDelegate : NSApplicationDelegate
	{
		public override void FinishedLaunching (MonoMac.Foundation.NSObject notification)
		{
			AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs a) =>  {
				if (a.Name.StartsWith("MonoMac")) {
					return typeof(MonoMac.AppKit.AppKitFramework).Assembly;
				}
				return null;
			};
			Program.RunGame();
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
		{
			return true;
		}
	}  
#endif
}

