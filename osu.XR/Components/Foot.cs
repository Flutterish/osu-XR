using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.XR;
using osu.Framework.XR.Components;
using osu.Framework.XR.Maths;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components {
	public class Foot : Model {
		public readonly Bindable<Vector3> TargetPosition = new Bindable<Vector3>();
		public readonly Bindable<Quaternion> TargetRotation = new Bindable<Quaternion>();
		public readonly Bindable<float> PositionToleranceBindable = new Bindable<float>( 0.22f );
		public readonly Bindable<float> RotationToleranceBindable = new Bindable<float>( 0.6f );

		Vector3 goalPosition = Vector3.Zero;
		Quaternion goalRotation = Quaternion.Identity;

		public Foot () {
			(TargetPosition, PositionToleranceBindable).BindValuesChanged( (pos, tol) => {
				if ( ( goalPosition - pos ).Length > tol ) {
					this.MoveTo( pos, 200, Easing.Out );

					goalPosition = pos;
				}
			}, true );

			(TargetRotation, RotationToleranceBindable).BindValuesChanged( (rot, tol) => {
				var current = ( goalRotation * new Vector4( 0, 0, 1, 1 ) ).Xyz;
				var goal = ( rot * new Vector4( 0, 0, 1, 1 ) ).Xyz;
				if ( ( goal - current ).Length > tol ) {
					this.RotateTo( rot, 100 );

					goalRotation = rot;
				}
			}, true );
		}
	}
}
