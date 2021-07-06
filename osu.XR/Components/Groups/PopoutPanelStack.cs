using osu.Framework.Graphics;
using osu.Framework.XR.Components;
using osu.XR.Components.Panels;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components.Groups {
	public abstract class PopoutPanelStack : MenuStack<FlatPanel> {
		public double TransitionDuration => 300;
		public Vector3 PopoutPosition = new Vector3( 0, 0, 0.02f );
		public float PopoutScale = 0.8f;
		protected override void ApplyTransformsTo ( FlatPanel panel, int index, float progress ) {
			if ( index == 0 ) {
				panel.FadeIn( TransitionDuration / 2 );
				panel.ScaleTo( Vector3.One, TransitionDuration, Easing.Out );
				panel.MoveTo( Vector3.Zero, TransitionDuration, Easing.Out );
			}
			else {
				panel.FadeOut( TransitionDuration, Easing.In );
				panel.ScaleTo( new Vector3( PopoutScale ), TransitionDuration, Easing.Out );
				panel.MoveTo( PopoutPosition, TransitionDuration, Easing.Out );
			}
		}
	}
}
