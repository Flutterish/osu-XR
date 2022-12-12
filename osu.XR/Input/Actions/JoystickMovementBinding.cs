using osu.Framework.Localisation;
using osu.XR.Localisation.Bindings.Types;

namespace osu.XR.Input.Actions;

public class JoystickMovementBinding : ActionBinding, IJoystickBinding {
	public override LocalisableString Name => JoystickStrings.Movement;

	public readonly Bindable<JoystickMovementType> MovementType = new( JoystickMovementType.Absolute );
	public readonly BindableDouble Distance = new( 100 ) { MinValue = 0, MaxValue = 100 };

	public JoystickMovementBinding () {
		TrackSetting( MovementType );
		TrackSetting( Distance );
	}
}

public enum JoystickMovementType {
	Absolute,
	Relative
}