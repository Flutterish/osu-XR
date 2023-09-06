using osu.XR.Input.Actions;

namespace osu.XR.Input.Handlers;

public partial class JoystickMovementHandler : JoystickHandler {
	public JoystickMovementHandler ( JoystickMovementBinding source ) : base( source.Parent?.Hand, source ) {
		MovementType.BindTo( source.MovementType );
		Distance.BindTo( source.Distance );
	}

	public readonly Bindable<JoystickMovementType> MovementType = new( JoystickMovementType.Absolute );
	public readonly BindableDouble Distance = new( 100 ) { MinValue = 0, MaxValue = 100 };

	protected override void Update () {
		base.Update();
		if ( MovementType.Value == JoystickMovementType.Absolute ) {
			MoveTo( JoystickPosition.Value * (float)Distance.Value / 100, isNormalized: true );
		}
		else if ( MovementType.Value == JoystickMovementType.Relative ) {
			MoveBy( JoystickPosition.Value * (float)(Distance.Value / 100 * Time.Elapsed / 100), isNormalized: true );
		}
	}

	protected void MoveTo ( Vector2 position, bool isNormalized = false ) {
		Target?.MoveTo( position, isNormalized );
	}
	protected void MoveBy ( Vector2 position, bool isNormalized = false ) {
		Target?.MoveBy( position, isNormalized );
	}
}
