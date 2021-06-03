using osu.Game.Overlays.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Inspector.Components.Editors {
	public class ToggleEditor : ValueEditor<bool> {
		SettingsCheckbox checkbox;
		public ToggleEditor ( bool value = default ) : base( value ) {
			Add( checkbox = new SettingsCheckbox {
				Current = Current
			} );
		}
	}
}
