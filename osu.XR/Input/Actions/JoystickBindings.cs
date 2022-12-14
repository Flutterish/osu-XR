using osu.Framework.Localisation;
using osu.Framework.XR.VirtualReality;
using osu.XR.Graphics.Bindings.Editors;
using osu.XR.Localisation.Bindings;

namespace osu.XR.Input.Actions;

public class JoystickBindings : CompositeActionBinding<IJoystickBinding>, IIsHanded {
	public override LocalisableString Name => Hand is Hand.Left ? TypesStrings.JoystickLeft : TypesStrings.JoystickRight;
	public override Drawable CreateEditor () => new JoystickEditor( this );

	public readonly Hand Hand;
	Hand IIsHanded.Hand => Hand;
	public JoystickBindings ( Hand hand ) {
		Hand = hand;
	}

	public override bool Add ( IJoystickBinding action ) {
		if ( base.Add( action ) ) {
			action.Parent = this;
			return true;
		}
		return false;
	}
}

public interface IJoystickBinding : IActionBinding {
	JoystickBindings? Parent { get; set; }
}