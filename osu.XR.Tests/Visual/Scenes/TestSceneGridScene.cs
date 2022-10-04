using osu.XR.Graphics.Scenes;

namespace osu.XR.Tests.Visual.Scenes;
public class TestSceneGridScene : Basic3DTestScene {
	public TestSceneGridScene () {
		Scene.Add( new GridScene() );
	}
}
