using osu.Game.Overlays.Settings;
using osu.XR.Graphics.Containers;
using osu.XR.Input.Actions;
using osu.XR.Localisation.Bindings;

namespace osu.XR.Graphics.Bindings.Editors;
public partial class JoystickEditor : FillFlowContainer {
	public JoystickEditor ( JoystickBindings source ) {
		AutoSizeAxes = Axes.Y;
		RelativeSizeAxes = Axes.X;
		Direction = FillDirection.Vertical;

		if ( source.Children.OfType<JoystickMovementBinding>().FirstOrDefault() is not JoystickMovementBinding movement ) {
			source.Add( movement = new() );
		}
		Add( new CollapsibleSection {
			Header = movement.Name,
			Child = movement.CreateEditor(),
			Expanded = movement.ShouldBeSaved
		} );

		Add( new SettingsButton {
			Depth = -1,
			Text = JoystickStrings.AddZone,
			Action = () => {
				expandSection = true;
				source.Add( new JoystickZoneBinding() );
				expandSection = false;
			}
		} );

		joystickBindings.BindTo( source.Children );
		joystickBindings.BindCollectionChanged( (_, e) => {
			if ( e.OldItems != null ) {
				foreach ( var zone in e.OldItems.OfType<JoystickZoneBinding>() ) {
					if ( zones.Remove( zone, out var drawable ) )
						Remove( drawable, disposeImmediately: true );
				}
			}
			if ( e.NewItems != null ) {
				foreach ( var zone in e.NewItems.OfType<JoystickZoneBinding>() ) {
					CollapsibleSection section = null!;
					Add( section = new CollapsibleSection {
						Header = zone.Name,
						Child = zone.CreateEditor(),
						Expanded = expandSection || zone.ShouldBeSaved
					} );
					zones.Add( zone, section );
				}
			}
		}, true );
	}

	BindableList<IJoystickBinding> joystickBindings = new();
	Dictionary<JoystickZoneBinding, Drawable> zones = new();
	bool expandSection;
}
