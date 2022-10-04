using osu.XR.Graphics.Panels.Menu;

namespace osu.XR.Tests.Visual.Panels;

public class TestSceneHandheldMenu : Osu3DTestScene {
	public TestSceneHandheldMenu () {
		Scene.Add( new HandheldMenu() );
	}
}
