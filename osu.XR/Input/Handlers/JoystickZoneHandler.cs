using osu.Framework.XR;
using osu.XR.Input.Actions;

namespace osu.XR.Input.Handlers;

public partial class JoystickZoneHandler : JoystickHandler {
	BindableInt count = new();
	BindableDouble startAngle = new();
	BindableDouble arc = new();
	BindableDouble deadzone = new();
	public List<BindableBool> Active = new();
	public JoystickZoneHandler ( JoystickZoneBinding source ) : base( source.Parent?.Hand, source ) {
		count.BindTo( source.Count );
		startAngle.BindTo( source.Offset );
		arc.BindTo( source.Arc );
		deadzone.BindTo( source.Deadzone );

		count.BindValueChanged( v => {
			while ( Active.Count < v.NewValue ) {
				BindableBool active = new();
				Active.Add( active );
				RegisterAction( source.Actions[Active.Count - 1], active );
			}
		}, true );
		(startAngle, arc, deadzone, JoystickPosition, count).BindValuesChanged( recalculate, true );
	}

	void recalculate () {
		for ( int i = 0; i < count.Value; i++ ) {
			Active[i].Value = IsNormalizedPointInside( JoystickPosition.Value, 360d / count.Value * i );
		}
	}

	// TODO this is copy-pasted from v1, we can probably remove some things
	double deltaAngle ( double current, double goal ) {
		var diff = ( goal - current ) % 360;
		if ( diff < 0 ) diff += 360;
		if ( diff > 180 ) diff -= 360;

		return diff;
	}

	public bool IsNormalizedPointInside ( Vector2 pos, double offset ) {
		if ( pos.Length < deadzone.Value ) return false;
		if ( pos.Length == 0 ) return true;
		return IsAngleInside( pos, offset );
	}
	public bool IsAngleInside ( Vector2 direction, double offset ) {
		var angle = (Math.Atan2( direction.Y, direction.X ) / Math.PI * 180) - offset;
		angle = deltaAngle( startAngle.Value, angle );
		if ( angle < 0 ) angle += 360;
		return angle <= arc.Value;
	}
}
