using osu.XR.Graphics.Sceneries.Components;

namespace osu.XR.Tests.Visual.Scenes;

public partial class TestSceneDust : Basic3DTestScene {
	public TestSceneDust () {
		Scene.Add( new DustEmitter() );
	}
}
