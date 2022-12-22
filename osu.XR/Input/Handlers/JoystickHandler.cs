using osu.Framework.Localisation;
using osu.Framework.XR.VirtualReality;

namespace osu.XR.Input.Handlers;

public abstract partial class JoystickHandler : ActionBindingHandler {
	public override LocalisableString Name => $@"{hand switch { Hand.Left => "Left ", Hand.Right => "Right ", _ => "" }}Joystick {Source.Name}";
	Hand? hand;

	public JoystickHandler ( Hand? hand, IActionBinding source ) : base( source ) {
		this.hand = hand;
		GetController( hand, c => {
			JoystickPosition.BindTo( Input.GetAction<Vector2Action>( VrAction.Scroll, c ).ValueBindable );
		} );
	}

	public readonly Bindable<Vector2> JoystickPosition = new();
}
