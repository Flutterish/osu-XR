using osu.Framework.Localisation;
using osu.Framework.XR.VirtualReality;

namespace osu.XR.Input.Handlers;

public abstract partial class JoystickHandler : ActionBindingHandler {
	public override LocalisableString Name => $@"{hand switch { Hand.Left => "Left ", Hand.Right => "Right ", _ => "" }}Joystick {Source.Name}";
	Hand? hand;

	public JoystickHandler ( Hand? hand, IActionBinding source ) : base( source ) {
		this.hand = hand;
		GetController( hand, c => {
			actualJoystickPosition.BindTo( Input.GetAction<Vector2Action>( VrAction.Scroll, c ).ValueBindable );
		} );

		actualJoystickPosition.BindValueChanged( v => {
			JoystickPosition.Value = new( v.NewValue.X, -v.NewValue.Y );
		} );
	}

	Bindable<Vector2> actualJoystickPosition = new();
	public readonly Bindable<Vector2> JoystickPosition = new();
}
