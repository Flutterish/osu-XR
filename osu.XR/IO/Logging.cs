using osu.Framework.Logging;

namespace osu.XR.IO;

static class Logging {
	public static void LogAs ( string name, string version ) {
		Logger.GameIdentifier = name;
		Logger.VersionIdentifier = version;
	}

	public static void LogAsOsuXr () => LogAs( "osu!xr", typeof(OsuXrGame).Assembly.GetName().Version?.ToString() ?? typeof( OsuXrGame ).Assembly.GetName().ToString() );
}
