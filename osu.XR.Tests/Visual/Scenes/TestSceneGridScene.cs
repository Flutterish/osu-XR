using osu.XR.Graphics.Sceneries;

namespace osu.XR.Tests.Visual.Scenes;
public partial class TestSceneGridScene : Basic3DTestScene {
	public TestSceneGridScene () {
		Scenery scenery = new();
		Scene.Add( scenery );
		scenery.Components.AddRange( GridScenery.CreateComponents() );
	}
}
