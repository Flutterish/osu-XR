using osu.Framework.Platform;
using osu.Framework;
using osu.XR.Tests;

using ( DesktopGameHost host = Host.GetSuitableDesktopHost( @"osu" ) ) {
	var browser = new OsuXrTestBrowser();
	host.Run( browser );
	return 0;
}