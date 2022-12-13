using osu.Framework.Localisation;
using osu.XR.Graphics.Bindings.Editors;
using osu.XR.Localisation.Bindings.Types;

namespace osu.XR.Input.Actions;

public class JoystickZoneBinding : ActionBinding, IJoystickBinding {
	public override LocalisableString Name => JoystickStrings.Zone;
	public override bool ShouldBeSaved => Action.Value != null;
	public override JoystickZoneEditor CreateEditor () => new JoystickZoneEditor( this );

	public readonly BindableDouble StartAngle = new( -30 );
	public readonly BindableDouble Arc = new( 60 ) { MinValue = 0, MaxValue = 360 };
	public readonly BindableDouble Deadzone = new( 0.4 ) { MinValue = 0, MaxValue = 1 };
	public readonly Bindable<object?> Action = new();

	public JoystickZoneBinding () {
		TrackSetting( StartAngle );
		TrackSetting( Arc );
		TrackSetting( Deadzone );
		TrackSetting( Action );
	}
}
