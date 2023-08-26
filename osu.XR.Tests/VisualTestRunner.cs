using osu.Framework.Platform;
using osu.Framework.XR;
using osu.XR.Tests;

Environment.SetEnvironmentVariable( "OSU_FRAME_STATISTICS_VIA_TOUCH", "0", EnvironmentVariableTarget.Process );
using ( DesktopGameHost host = HostXR.GetSuitableDesktopHost( "osu" ) ) {
	var browser = new OsuXrTestBrowser();
	host.Run( browser );
	return 0;
}