using osu.XR.Input.Actions;

namespace osu.XR.Input.Handlers;

public partial class JoystickMovementHandler : JoystickHandler {
	public JoystickMovementHandler ( JoystickMovementBinding source ) : base( source.Parent?.Hand, source ) { }
}
