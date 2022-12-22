using osu.Framework.Localisation;
using osu.XR.Graphics.Bindings.Editors;
using osu.XR.Input.Handlers;
using osu.XR.IO;
using osu.XR.Localisation.Bindings.Types;
using System.Text.Json;

namespace osu.XR.Input.Actions;

public class JoystickZoneBinding : ActionBinding, IJoystickBinding {
	public override LocalisableString Name => JoystickStrings.Zone;
	public override bool ShouldBeSaved => Action.ShouldBeSaved;
	public JoystickBindingType Type => JoystickBindingType.Zone;
	public JoystickBindings? Parent { get; set; }
	public override JoystickZoneEditor CreateEditor () => new JoystickZoneEditor( this );
	public override JoystickZoneHandler CreateHandler () => new( this );

	public readonly BindableDouble StartAngle = new( -30 );
	public readonly BindableDouble Arc = new( 60 ) { MinValue = 0, MaxValue = 360 };
	public readonly BindableDouble Deadzone = new( 0.4 ) { MinValue = 0, MaxValue = 1 };
	public readonly RulesetAction Action = new();

	public JoystickZoneBinding () {
		TrackSetting( StartAngle );
		TrackSetting( Arc );
		TrackSetting( Deadzone );
		TrackSetting( Action );
	}

	public override object CreateSaveData ( BindingsSaveContext context ) => new SaveData {
		StartAngle = StartAngle.Value,
		Arc = Arc.Value,
		Deadzone = Deadzone.Value,
		Action = context.SaveAction( Action )
	};

	public static JoystickZoneBinding? Load ( JsonElement data, BindingsSaveContext ctx ) => Load<JoystickZoneBinding, SaveData>( data, ctx, static (save, ctx) => {
		var zone = new JoystickZoneBinding();
		ctx.LoadAction( zone.Action, save.Action );
		zone.StartAngle.Value = save.StartAngle;
		zone.Arc.Value = save.Arc;
		zone.Deadzone.Value = save.Deadzone;
		return zone;
	} );

	public struct SaveData {
		public double StartAngle;
		public double Arc;
		public double Deadzone;
		public ActionData? Action;
	}
}
