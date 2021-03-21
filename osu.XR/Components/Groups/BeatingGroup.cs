using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.XR.Components;
using osu.XR.Drawables;
using osuTK;

namespace osu.XR.Components.Groups {
	public class BeatingGroup : Container3D {
		public readonly Bindable<Beat> BindableBeat = new();

		public BeatingGroup () {
			BindableBeat.ValueChanged += v => {
				FinishTransforms();
				this.ScaleTo( new Vector3( (float)( 1 - v.NewValue.AverageAmplitude * 0.06 ) ), 50 ).Then().ScaleTo( Vector3.One, 100 );
			};
		}

		[BackgroundDependencyLoader]
		private void load ( BeatProvider beat ) {
			BindableBeat.BindTo( beat.BindableBeat );
		}
	}
}
