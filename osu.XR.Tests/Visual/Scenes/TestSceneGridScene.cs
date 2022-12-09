using osu.XR.Graphics.Sceneries;

namespace osu.XR.Tests.Visual.Scenes;
public partial class TestSceneGridScene : Basic3DTestScene {
	public TestSceneGridScene () {
		Scene.Add( new GridScenery() );
	}
}
