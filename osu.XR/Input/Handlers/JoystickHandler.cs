using osu.Framework.XR.VirtualReality;

namespace osu.XR.Input.Handlers;

public abstract partial class JoystickHandler : ActionBindingHandler {
	public JoystickHandler ( Hand? hand ) {
		GetController( hand, c => {
			JoystickPosition.BindTo( Input.GetAction<Vector2Action>( VrAction.Scroll, c ).ValueBindable );
		} );
	}

	public readonly Bindable<Vector2> JoystickPosition = new();
}
