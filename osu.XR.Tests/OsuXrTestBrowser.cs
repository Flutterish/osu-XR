using osu.Framework.Allocation;
using osu.Framework.Platform;

namespace osu.XR.Tests;

public partial class OsuXrTestBrowser : OsuXrGameBase {
	[BackgroundDependencyLoader]
	private void load ( GameHost host ) {
		var browser = new osu.Framework.XR.Testing.TestBrowser();
		browser.SetHost( host );
		Add( browser );
	}
}
