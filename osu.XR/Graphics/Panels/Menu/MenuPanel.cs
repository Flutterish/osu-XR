using osu.Game.Overlays;

namespace osu.XR.Graphics.Panels.Menu;

public class MenuPanel : OsuPanel {
	[Cached]
	protected OverlayColourProvider ColourProvider { get; } = new OverlayColourProvider( OverlayColourScheme.Purple );

	protected const float ASPECT_RATIO = 4f / 5f;
	protected const float PREFFERED_CONTENT_WIDTH = 400;
	protected const float PREFFERED_CONTENT_HEIGHT = PREFFERED_CONTENT_WIDTH / ASPECT_RATIO;

	public MenuPanel () {
		ContentSize = new( PREFFERED_CONTENT_WIDTH, PREFFERED_CONTENT_HEIGHT );
	}

	protected override void RegenrateMesh () {
		var w = 0.4f / 2;
		var h = w / ASPECT_RATIO;

		Mesh.AddQuad( new Quad3 {
			TL = new Vector3( -w, h, 0 ),
			TR = new Vector3( w, h, 0 ),
			BL = new Vector3( -w, -h, 0 ),
			BR = new Vector3( w, -h, 0 )
		} );
	}
}
