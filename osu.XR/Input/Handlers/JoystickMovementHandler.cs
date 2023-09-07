using osu.XR.Input.Actions;

namespace osu.XR.Input.Handlers;

public partial class JoystickMovementHandler : JoystickHandler {
	public JoystickMovementHandler ( JoystickMovementBinding source ) : base( source.Parent?.Hand, source ) {
		MovementType.BindTo( source.MovementType );
		Distance.BindTo( source.Distance );
	}

	public readonly Bindable<JoystickMovementType> MovementType = new( JoystickMovementType.Absolute );
	public readonly BindableDouble Distance = new( 100 ) { MinValue = 0, MaxValue = 100 };

	InjectedInputCursor? cursor;
	protected override void Update () {
		base.Update();
		if ( Target == null )
			return;

		if ( (MovementType.Value == JoystickMovementType.None) != (cursor == null) ) {
			if ( cursor == null ) {
				cursor = Target.GetCursor( this );
				cursor.Colour = Hand is Framework.XR.VirtualReality.Hand.Left ? Colour4.Cyan : Colour4.Orange;
			}
			else {
				Target.DeleteCursor( this );
				cursor = null;
			}
		}

		if ( cursor == null )
			return;

		if ( MovementType.Value == JoystickMovementType.Absolute ) {
			cursor.MoveTo( JoystickPosition.Value * (float)Distance.Value / 100, isNormalized: true );
		}
		else if ( MovementType.Value == JoystickMovementType.Relative ) {
			cursor.MoveBy( JoystickPosition.Value * (float)(Distance.Value / 100 * Time.Elapsed / 100), isNormalized: true );
		}
	}
}
