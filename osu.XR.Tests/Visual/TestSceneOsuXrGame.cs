using osu.Framework.Testing;

namespace osu.XR.Tests.Visual;

public partial class TestSceneOsuXrGame : TestScene {
	protected override void LoadComplete () {
		AddGame( new OsuXrGame( true ) );
	}
}
