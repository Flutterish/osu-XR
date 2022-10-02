using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.XR.Graphics.Panels;
using osuTK.Graphics;
namespace osu.XR.Tests.Visual.Panels;

public class TestSceneCurvedPanel : Basic3DTestScene {
	CurvedPanel panel;
	public TestSceneCurvedPanel () {
		Scene.Add( panel = new CurvedPanel {
			ContentSize = new( 500 )
		} );
		panel.Content.Add( new Box { RelativeSizeAxes = Axes.Both, Colour = Color4.Gray } );
		panel.Content.Add( new SpriteText { Text = "Hello, World!" } );
	}
}
