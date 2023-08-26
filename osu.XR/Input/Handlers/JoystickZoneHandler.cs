using osu.Framework.XR;
using osu.XR.Input.Actions;

namespace osu.XR.Input.Handlers;

public partial class JoystickZoneHandler : JoystickHandler {
	BindableDouble startAngle = new();
	BindableDouble arc = new();
	BindableDouble deadzone = new();
	public JoystickZoneHandler ( JoystickZoneBinding source ) : base( source.Parent?.Hand, source ) {
		startAngle.BindTo( source.StartAngle );
		arc.BindTo( source.Arc );
		deadzone.BindTo( source.Deadzone );

		(startAngle, arc, deadzone, JoystickPosition).BindValuesChanged( recalculate, true );
		RegisterAction( source.Action, Active );
	}

	void recalculate () {
		Active.Value = IsNormalizedPointInside( JoystickPosition.Value );
	}

	// TODO this is copy-pasted from v1, we can probably remove some things
	double deltaAngle ( double current, double goal ) {
		var diff = ( goal - current ) % 360;
		if ( diff < 0 ) diff += 360;
		if ( diff > 180 ) diff -= 360;

		return diff;
	}

	public bool IsNormalizedPointInside ( Vector2 pos ) {
		if ( pos.Length < deadzone.Value ) return false;
		if ( pos.Length == 0 ) return true;
		return IsAngleInside( pos );
	}
	public bool IsAngleInside ( Vector2 direction ) {
		var angle = Math.Atan2( direction.Y, direction.X ) / Math.PI * 180;
		angle = deltaAngle( startAngle.Value, angle );
		if ( angle < 0 ) angle += 360;
		return angle <= arc.Value;
	}

	public readonly BindableBool Active = new();
}
