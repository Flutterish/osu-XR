using osu.Game.Overlays.Settings;
using osu.XR.Graphics.Containers;
using osu.XR.Input.Actions;
using osu.XR.Localisation.Bindings;

namespace osu.XR.Graphics.Bindings.Editors;
public partial class JoystickEditor : FillFlowContainer {
	JoystickMovementBinding movement = new();
	public JoystickEditor ( JoystickBindings source ) {
		AutoSizeAxes = Axes.Y;
		RelativeSizeAxes = Axes.X;
		Direction = FillDirection.Vertical;

		Add( new CollapsibleSection {
			Header = movement.Name,
			Child = movement.CreateEditor()
		} );

		Add( new SettingsButton {
			Depth = -1,
			Text = JoystickStrings.AddZone,
			Action = () => {
				var zone = new JoystickZoneBinding();
				CollapsibleSection section = null!;
				Add( section = new CollapsibleSection {
					Header = zone.Name,
					Child = zone.CreateEditor().With( x => x.RemoveRequested += x => {
						Remove( section, disposeImmediately: true );
					} ),
					Expanded = true
				} );
			}
		} );
	}
}
