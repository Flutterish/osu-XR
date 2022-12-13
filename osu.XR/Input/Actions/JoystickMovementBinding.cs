using osu.Framework.Localisation;
using osu.XR.Graphics.Bindings.Editors;
using osu.XR.Localisation.Bindings.Joystick.Movement;
using osu.XR.Localisation.Bindings.Types;

namespace osu.XR.Input.Actions;

public class JoystickMovementBinding : ActionBinding, IJoystickBinding {
	public override LocalisableString Name => JoystickStrings.Movement;
	public override bool ShouldBeSaved => MovementType.Value != JoystickMovementType.None;
	public override Drawable CreateEditor () => new JoystickMovementEditor( this );

	public readonly Bindable<JoystickMovementType> MovementType = new( JoystickMovementType.None );
	public readonly BindableDouble Distance = new( 100 ) { MinValue = 0, MaxValue = 100 };

	public JoystickMovementBinding () {
		TrackSetting( MovementType );
		TrackSetting( Distance );
	}
}

public enum JoystickMovementType {
	[LocalisableDescription(typeof(TypesStrings), nameof(TypesStrings.None))]
	None,

	[LocalisableDescription(typeof(TypesStrings), nameof(TypesStrings.Absolute))]
	Absolute,

	[LocalisableDescription(typeof(TypesStrings), nameof(TypesStrings.Relative))]
	Relative
}