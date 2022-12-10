using osu.Framework.XR;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Transforms;

namespace osu.XR.Graphics.Player;

public partial class Foot : Model {
	public readonly Bindable<Vector3> TargetPosition = new();
	public readonly Bindable<Quaternion> TargetRotation = new();
	public readonly Bindable<float> PositionToleranceBindable = new( 0.22f );
	public readonly Bindable<float> RotationToleranceBindable = new( 0.6f );

	Vector3 goalPosition = Vector3.Zero;
	Quaternion goalRotation = Quaternion.Identity;

	public Foot () {
		(TargetPosition, PositionToleranceBindable).BindValuesChanged( ( pos, tol ) => {
			if ( ( goalPosition - pos ).Length > tol ) {
				this.MoveTo( pos, 200, Easing.Out );

				goalPosition = pos;
			}
		}, true );

		(TargetRotation, RotationToleranceBindable).BindValuesChanged( ( rot, tol ) => {
			var current = ( goalRotation * new Vector4( 0, 0, 1, 1 ) ).Xyz;
			var goal = ( rot * new Vector4( 0, 0, 1, 1 ) ).Xyz;
			if ( ( goal - current ).Length > tol ) {
				this.RotateTo( rot, 100 );

				goalRotation = rot;
			}
		}, true );
	}
}
