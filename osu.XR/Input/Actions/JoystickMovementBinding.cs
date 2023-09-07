using osu.Framework.Localisation;
using osu.XR.Graphics.Bindings.Editors;
using osu.XR.Input.Handlers;
using osu.XR.Input.Migration;
using osu.XR.IO;
using osu.XR.Localisation.Bindings.Joystick.Movement;
using osu.XR.Localisation.Bindings.Types;
using System.Text.Json;

namespace osu.XR.Input.Actions;

public class JoystickMovementBinding : ActionBinding, IJoystickBinding {
	public override LocalisableString Name => JoystickStrings.Movement;
	public override bool ShouldBeSaved => MovementType.Value != JoystickMovementType.None;
	public JoystickBindingType Type => JoystickBindingType.Movement;
	public JoystickBindings? Parent { get; set; }
	public override Drawable CreateEditor () => new JoystickMovementEditor( this );
	public override JoystickMovementHandler CreateHandler () => new( this );

	public readonly Bindable<JoystickMovementType> MovementType = new( JoystickMovementType.None );
	public readonly BindableDouble Distance = new( 100 ) { MinValue = 0, MaxValue = 100 };

	public JoystickMovementBinding () {
		TrackSetting( MovementType );
		TrackSetting( Distance );
	}

	protected override object CreateSaveData ( BindingsSaveContext context ) => new SaveData {
		Type = MovementType.Value,
		Distance = Distance.Value
	};

	public static JoystickMovementBinding? Load ( JsonElement data, BindingsSaveContext ctx ) => Load<JoystickMovementBinding, SaveData>( data, ctx, static (save, ctx) => {
		var movement = new JoystickMovementBinding();
		movement.MovementType.Value = save.Type;
		movement.Distance.Value = save.Distance;
		return movement;
	} );

	[FormatVersion( "" )]
	[FormatVersion( "[Initial]" )]
	public struct SaveData {
		public JoystickMovementType Type;
		public double Distance;
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