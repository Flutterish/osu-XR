using osu.XR.Graphics.Sceneries.Components;

namespace osu.XR.Tests.Visual.Rendering;

public partial class TestSceneSkybox : Basic3DTestScene {
	public TestSceneSkybox () {
		Scene.Add( new VerticalGradientSkyBox() );
	}
}
