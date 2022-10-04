using osu.Framework.XR.Graphics.Rendering;
using osu.XR.Graphics.Panels;

namespace osu.XR.Tests.Visual.Osu;

public class TestSceneOsuGame : Basic3DTestScene {
	OsuGamePanel osuPanel;

	public TestSceneOsuGame () {
		Scene.Camera.Z = -5;
		Scene.Add( osuPanel = new() {
			ContentSize = new( 1920, 1080 )
		} );

		AddToggleStep( "Use Touch", v => {
			MouseInteractionSource.UseTouch = v;
		} );
	}
}
