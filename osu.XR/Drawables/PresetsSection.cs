using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Game.Overlays.Settings;
using osu.XR.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Drawables {
	public class PresetsSection : SettingsSection {
		public override string Header => "Presets";

		public override Drawable CreateIcon () => new SpriteIcon {
			Icon = FontAwesome.Solid.BoxOpen
		};

		SettingsPreset<XrConfigSetting> lastPreset;

		private List<(string name, SettingsPreset<XrConfigSetting> preset)> presets = new() {
			("Default", XrConfigManager.DefaultPreset),
			("Touchscreen Big", XrConfigManager.PresetTouchscreenBig),
			("Touchscreen Small", XrConfigManager.PresetTouchscreenSmall)
		};

		// TODO allow users to save their own presets
		[BackgroundDependencyLoader]
		private void load ( XrConfigManager config ) {
			Children = presets.Select( x => new SettingsButton {
				Text = x.name,
				Action = () => {
					lastPreset = new( config, XrConfigManager.DefaultPreset );
					x.preset.Load( config );
				}
			} ).Append( new SettingsButton {
				Text = "Previous",
				Action = () => {
					lastPreset?.Load( config );
				}
			} ).Append( new SettingsButton {
				Text = "Print Current (Runtime Logs)",
				Action = () => {
					var preset = new SettingsPreset<XrConfigSetting>( config, XrConfigManager.DefaultPreset );
					foreach ( var (k,v) in preset.values ) {
						Logger.Log( $"{k}: {v}" );
					}
				}
			} ).ToArray();
		}
	}
}
