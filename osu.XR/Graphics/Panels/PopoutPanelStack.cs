using osu.Framework.XR.Graphics.Panels;
using osu.Framework.XR.Graphics.Transforms;

namespace osu.XR.Graphics.Panels;

public class PopoutPanelStack<T> : PanelStack<T> where T : Panel {
	public double TransitionDuration = 300;
	public Vector3 PopoutPosition = new Vector3( 0, 0, 0.02f );
	public float PopoutScale = 0.8f;

	protected override void PerformLayout ( ReadOnlySpan<T> children ) {
		if ( children.Length != 0 ) {
			var first = children[0];
			first.FadeIn( TransitionDuration / 2 );
			first.ScaleTo( Vector3.One, TransitionDuration, Easing.Out );
			first.MoveTo( Vector3.Zero, TransitionDuration, Easing.Out );
		}
		foreach ( var i in children[1..] ) {
			i.FadeOut( TransitionDuration, Easing.In );
			i.ScaleTo( new Vector3( PopoutScale ), TransitionDuration, Easing.Out );
			i.MoveTo( PopoutPosition, TransitionDuration, Easing.Out );
		}
	}
}