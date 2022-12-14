using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Testing.VirtualReality;

namespace osu.XR.Tests;

public partial class OsuXrTestBrowser : OsuXrGameBase {
	[BackgroundDependencyLoader]
	private void load ( GameHost host ) {
		var browser = new osu.Framework.XR.Testing.TestBrowser();
		browser.SetHost( host );
		Add( browser );

		if ( Compositor is TestingVrCompositor comp ) {
			var noScene = new Scene();
			var rig = new TestingRig( noScene );
			comp.AddRig( rig );
		}
	}
}
