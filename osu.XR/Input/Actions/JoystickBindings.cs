using osu.Framework.Localisation;
using osu.Framework.XR.VirtualReality;
using osu.XR.Graphics.Bindings.Editors;
using osu.XR.Input.Migration;
using osu.XR.IO;
using osu.XR.Localisation.Bindings;
using System.Data;
using System.Text.Json;

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

	protected override object CreateSaveData ( IEnumerable<IJoystickBinding> children, BindingsSaveContext context ) => new SaveData {
		Type = BindingType.Joystick,
		Hand = Hand,
		Data = children.Select( x => new ChildSaveData { Type = x.Type, Data = x.CreateSaveData( context ) } as object ).ToArray()
	};

	public static JoystickBindings? Load ( JsonElement data, BindingsSaveContext ctx ) => Load<JoystickBindings, SaveData>( data, ctx, static (save, ctx) => {
		var joystick = new JoystickBindings( save.Hand );
		joystick.LoadChildren<ChildSaveData>( save.Data, ctx, static (save, ctx) => save.Type switch {
			JoystickBindingType.Movement => JoystickMovementBinding.Load( (JsonElement)save.Data, ctx ),
			JoystickBindingType.Zone => JoystickZoneBinding.Load( (JsonElement)save.Data, ctx ),
			_ => null
		} );
		return joystick;
	} );

	[MigrateFrom(typeof(V1SaveData), "[Initial]")]
	public struct SaveData {
		public BindingType Type;
		public Hand Hand;
		public object[] Data;

		public static implicit operator SaveData ( V1SaveData from ) => new() {
			Type = BindingType.Joystick,
			Hand = from.Type == "Left Joystick" ? Hand.Left : Hand.Right,
			Data = from.Data
		};
	}

	public struct V1SaveData {
		public string Type;
		public object[] Data;
	}

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