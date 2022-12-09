using osu.XR.Graphics.Sceneries;

namespace osu.XR.Tests.Visual.Scenes;
public partial class TestSceneLightsOutScene : Basic3DTestScene {
	public TestSceneLightsOutScene () {
		Scene.Add( new LightsOutScenery() );
	}
}
