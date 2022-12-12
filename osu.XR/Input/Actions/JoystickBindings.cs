using osu.Framework.Localisation;
using osu.Framework.XR.VirtualReality;
using osu.XR.Localisation.Bindings;

namespace osu.XR.Input.Actions;

public class JoystickBindings : CompositeActionBinding {
	public override LocalisableString Name => Hand is Hand.Left ? TypesStrings.JoystickLeft : TypesStrings.JoystickRight;

	public readonly Hand Hand;
	public JoystickBindings ( Hand hand ) {
		Hand = hand;
	}
}

public interface IJoystickBinding {

}