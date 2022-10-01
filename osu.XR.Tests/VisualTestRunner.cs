using osu.Framework.Platform;
using osu.Framework;

using ( DesktopGameHost host = Host.GetSuitableDesktopHost( @"osu!XR" ) ) {
	var browser = new osu.Framework.XR.Testing.TestBrowser();
	host.Run( browser );
	return 0;
}