using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK;

namespace osu.XR.Panels.Overlays {
	public abstract class PanelOverlay : CompositeDrawable {
		public PanelOverlay () {
			Origin = Anchor.TopCentre;
			Anchor = Anchor.BottomCentre;

			RelativeSizeAxes = Axes.Both;
			AddInternal( new Box { RelativeSizeAxes = Axes.Both, Colour = OsuColour.Gray( 0.04f ) } );
		}

		public override void Hide () {
			this.TransformTo( nameof( RelativeAnchorPosition ), new Vector2( 0.5f, 1 ), 500, Easing.Out );
		}

		public override void Show () {
			this.TransformTo( nameof( RelativeAnchorPosition ), new Vector2( 0.5f, 0 ), 500, Easing.Out );
		}
	}
}
