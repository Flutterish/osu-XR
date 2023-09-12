using osu.Framework.Localisation;
using osu.XR.Graphics.Bindings.Editors;
using osu.XR.Input.Handlers;
using osu.XR.Input.Migration;
using osu.XR.IO;
using osu.XR.Localisation.Bindings.Types;
using System.Text.Json;

namespace osu.XR.Input.Actions;

public class JoystickZoneBinding : ActionBinding, IJoystickBinding {
	public override LocalisableString Name => JoystickStrings.Zone;
	public override bool ShouldBeSaved => Actions.Take( Count.Value ).Any( x => x.ShouldBeSaved );
	public JoystickBindingType Type => JoystickBindingType.Zone;
	public JoystickBindings? Parent { get; set; }
	public override JoystickZoneEditor CreateEditor () => new JoystickZoneEditor( this );
	public override JoystickZoneHandler CreateHandler () => new( this );

	public readonly BindableInt Count = new( 1 ) { MinValue = 1, MaxValue = 8 }; // we keep a separate count because it allows limits and prevents accidental removals of actions
	public readonly BindableDouble Offset = new( -30 );
	public readonly BindableDouble Arc = new( 60 ) { MinValue = 0, MaxValue = 360 };
	public readonly BindableDouble Deadzone = new( 0.4 ) { MinValue = 0, MaxValue = 1 };
	BindableList<RulesetAction> actions = new();
	public BindableList<RulesetAction> Actions {
		get {
			while ( actions.Count < Count.Value ) {
				actions.Add( new() );
			}
			return actions;
		}
	}

	public JoystickZoneBinding () {
		TrackSetting( Offset );
		TrackSetting( Arc );
		TrackSetting( Deadzone );

		Actions.BindCollectionChanged( (_,e) => {
			if ( e.OldItems != null ) {
				foreach ( RulesetAction i in e.OldItems ) {
					i.ValueChanged -= onActionValueChanged;
				}
			}
			if ( e.NewItems != null ) {
				foreach ( RulesetAction i in e.NewItems ) {
					i.ValueChanged += onActionValueChanged;
				}
			}
		}, true );
	}

	private void onActionValueChanged ( ValueChangedEvent<object?> obj ) {
		OnSettingsChanged();
	}

	protected override object CreateSaveData ( BindingsSaveContext context ) => new SaveData {
		Offset = Offset.Value,
		Arc = Arc.Value,
		Deadzone = Deadzone.Value,
		Count = Count.Value,
		Actions = Actions.Take( Count.Value ).Select( context.SaveAction ).ToArray()
	};

	public static JoystickZoneBinding? Load ( JsonElement data, BindingsSaveContext ctx ) => Load<JoystickZoneBinding, SaveData>( data, ctx, static (save, ctx) => {
		var zone = new JoystickZoneBinding();

		zone.Count.Value = save.Count;
		zone.Offset.Value = save.Offset;
		zone.Arc.Value = save.Arc;
		zone.Deadzone.Value = save.Deadzone;
		zone.actions.Clear();
		zone.actions.AddRange( save.Actions.Select( x => {
			var action = new RulesetAction();
			ctx.LoadAction( action, x );
			return action;
		} ) );
		return zone;
	} );

	[FormatVersion( "Range" )]
	public struct SaveData {
		public int Count;
		public double Offset;
		public double Arc;
		public double Deadzone;
		public ActionData?[] Actions;

		public static implicit operator SaveData ( SaveDataSingle single ) => new() {
			Count = 1,
			Offset = single.StartAngle,
			Arc = single.Arc,
			Deadzone = single.Deadzone,
			Actions = new[] { single.Action }
		};
	}

	[FormatVersion( "" )]
	[FormatVersion( "[Initial]" )]
	public struct SaveDataSingle {
		public double StartAngle;
		public double Arc;
		public double Deadzone;
		public ActionData? Action;
	}
}
