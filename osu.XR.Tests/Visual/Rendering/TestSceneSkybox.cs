using osu.XR.Graphics.Scenes;

namespace osu.XR.Tests.Visual.Rendering;

public class TestSceneSkybox : Basic3DTestScene {
	public TestSceneSkybox () {
		Scene.Add( new VerticalGradientSkyBox() );
	}
}
