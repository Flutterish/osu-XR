using osu.Framework.XR.Graphics.Rendering;
using osu.XR.Graphics.Panels;
using osu.XR.Osu;

namespace osu.XR.Tests.Visual.Osu;

public class TestSceneOsuGame : Basic3DTestScene {
	CurvedPanel osuPanel;
	OsuGameContainer gameContainer;

	public TestSceneOsuGame () {
		Scene.Camera.Z = -5;
		Scene.Add( osuPanel = new() {
			ContentSize = new( 1920, 1080 )
		} );
		osuPanel.Content.Add( gameContainer = new() );
	}
}
