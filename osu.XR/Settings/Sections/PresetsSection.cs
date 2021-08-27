using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Game.Overlays.Settings;
using System.Collections.Generic;
using System.Linq;

namespace osu.XR.Settings.Sections {
	public class PresetsSection : SettingsSection {
		public override string DisplayName => "Presets";
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
			AddRange( presets.Select( x => new SettingsButton {
				Text = x.name,
				Action = () => {
					lastPreset = new( config, XrConfigManager.TypeLookpuPreset );
					x.preset.Load( config, XrConfigManager.TypeLookpuPreset );
				}
			} ).Append( new SettingsButton {
				Text = "Previous",
				Action = () => {
					lastPreset?.Load( config, XrConfigManager.TypeLookpuPreset );
				}
			} ) );

			if ( System.Diagnostics.Debugger.IsAttached ) {
				Add( new SettingsButton {
					Text = "Print Current (Runtime Logs)",
					Action = () => {
						var preset = new SettingsPreset<XrConfigSetting>( config, XrConfigManager.TypeLookpuPreset );
						foreach ( var (k, v) in preset.values ) {
							Logger.Log( $"{k}: {v}" );
						}
					}
				} );
			}
		}
	}
}
