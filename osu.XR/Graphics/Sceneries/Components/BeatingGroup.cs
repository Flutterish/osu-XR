using osu.Framework.XR.Graphics.Containers;
using osu.Framework.XR.Graphics.Transforms;
using osu.XR.Timing;

namespace osu.XR.Graphics.Sceneries.Components;

public partial class BeatingGroup : Container3D {
	Bindable<Beat> bindableBeat = new();
	public BeatingGroup () {
		bindableBeat.ValueChanged += v => {
			FinishTransforms();
			this.ScaleTo( new Vector3( (float)( 1 - v.NewValue.AverageAmplitude * 0.06 ) ), 50 ).Then().ScaleTo( Vector3.One, 100 );
		};
	}

	[BackgroundDependencyLoader]
	private void load ( BeatSyncSource beat ) {
		bindableBeat.BindTo( beat.BindableBeat );
	}
}
