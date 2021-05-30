using osu.Framework;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Game.Tests;
using System;

namespace osu.XR.Tests {
	public static class VisualTestRunner {
		[STAThread]
		public static int Main ( string[] args ) {
			using ( DesktopGameHost host = Host.GetSuitableHost( @"osu", true ) ) {
				var browser = new OsuTestBrowser();
				browser.OnLoadComplete += _ => {
					browser.Resources.AddStore( new DllResourceStore( osu.Framework.XR.Resources.ResourceAssembly ) );
				};
				host.Run( browser );
				return 0;
			}
		}
	}
}
