using osu.Framework.Testing;

namespace osu.XR.Tests.Visual;

public class TestSceneOsuXrGame : TestScene {
	protected override void LoadComplete () {
		AddGame( new OsuXrGame( true ) );
	}
}
