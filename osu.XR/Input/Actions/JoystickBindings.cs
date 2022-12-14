using osu.Framework.Localisation;
using osu.Framework.XR.VirtualReality;
using osu.XR.Graphics.Bindings.Editors;
using osu.XR.IO;
using osu.XR.Localisation.Bindings;

namespace osu.XR.Input.Actions;

public class JoystickBindings : CompositeActionBinding<IJoystickBinding>, IHasBindingType, IIsHanded {
	public override LocalisableString Name => Hand is Hand.Left ? TypesStrings.JoystickLeft : TypesStrings.JoystickRight;
	public override Drawable CreateEditor () => new JoystickEditor( this );

	public readonly Hand Hand;
	Hand IIsHanded.Hand => Hand;
	public BindingType Type => BindingType.Joystick;
	public JoystickBindings ( Hand hand ) {
		Hand = hand;
	}

	public override bool Add ( IJoystickBinding action ) {
		if ( action.Type is JoystickBindingType.Movement && Children.Any( x => x.Type == JoystickBindingType.Movement ) )
			return false;

		if ( base.Add( action ) ) {
			action.Parent = this;
			return true;
		}
		return false;
	}

	protected override object CreateSaveData ( IEnumerable<IJoystickBinding> children, BindingsSaveContext context )
		=> children.Select( x => new ChildSaveData { Type = x.Type, Data = x.CreateSaveData( context ) } ).ToArray();

	public struct ChildSaveData {
		public JoystickBindingType Type;
		public object Data;
	}
}

public enum JoystickBindingType {
	Movement,
	Zone
}

public interface IJoystickBinding : IActionBinding {
	JoystickBindingType Type { get; }
	JoystickBindings? Parent { get; set; }
}