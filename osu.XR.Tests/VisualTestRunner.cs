using osu.Framework;
using osu.Framework.Platform;
using osu.Framework.XR;
using osu.XR.Tests;

using ( DesktopGameHost host = HostXR.GetSuitableDesktopHost( "osu" ) ) {
	var browser = new OsuXrTestBrowser();
	host.Run( browser );
	return 0;
}