using osu.Framework.Graphics;
using osu.Framework.XR.Components;
using osu.XR.Components.Panels;
using osuTK;

namespace osu.XR.Components.Groups {
	public abstract class RegularPanelStack : MenuStack<FlatPanel> {
		public double TransitionDuration => 300;
		public Vector3 PanelOffset = new Vector3( 0.01f, 0, 0.02f );
		protected override void ApplyTransformsTo ( FlatPanel panel, int index, float progress ) {
			panel.MoveTo( PanelOffset * index, TransitionDuration, Easing.Out );
		}
	}
}
