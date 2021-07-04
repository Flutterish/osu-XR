using osu.Game.Overlays.Settings;

namespace osu.XR.Inspector.Editors {
	public class ToggleEditor : ValueEditor<bool> {
		SettingsCheckbox checkbox;
		public ToggleEditor ( bool value = default ) : base( value ) {
			Add( checkbox = new SettingsCheckbox {
				Current = Current
			} );
		}
	}
}
