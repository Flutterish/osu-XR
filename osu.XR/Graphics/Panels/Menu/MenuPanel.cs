using osu.Game.Overlays;

namespace osu.XR.Graphics.Panels.Menu;

public partial class MenuPanel : OsuPanel {
	[Cached]
	protected OverlayColourProvider ColourProvider { get; } = new OverlayColourProvider( OverlayColourScheme.Purple );

	protected const float ASPECT_RATIO = 4f / 5f;
	protected const float PREFFERED_CONTENT_WIDTH = 400;
	protected const float PREFFERED_CONTENT_HEIGHT = PREFFERED_CONTENT_WIDTH / ASPECT_RATIO;

	public const float PANEL_WIDTH = 0.4f;
	public const float PANEL_HEIGHT = PANEL_WIDTH / ASPECT_RATIO;

	public MenuPanel () {
		ContentSize = new( PREFFERED_CONTENT_WIDTH, PREFFERED_CONTENT_HEIGHT );
	}

	float parentAlpha = 1;
	public float ParentAlpha {
		get => parentAlpha;
		set {
			parentAlpha = value;
			base.Alpha = Alpha * parentAlpha;
		}
	}

	float alpha = 1;
	public override float Alpha {
		get => alpha;
		set {
			alpha = value;
			base.Alpha = Alpha * parentAlpha;
		}
	}

	protected override void RegenrateMesh () {
		var w = PANEL_WIDTH / 2;
		var h = PANEL_HEIGHT / 2;

		Mesh.AddQuad( new Quad3 {
			TL = new Vector3( -w, h, 0 ),
			TR = new Vector3( w, h, 0 ),
			BL = new Vector3( -w, -h, 0 ),
			BR = new Vector3( w, -h, 0 )
		} );
	}
}
