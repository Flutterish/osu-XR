using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Game.Overlays.Settings;
using osu.XR.Components.Groups;
using osu.XR.Settings;
using System.Collections.Generic;
using System.Linq;

namespace osu.XR.Drawables {
	public class PresetsSection : FillFlowContainer, IHasName, IHasIcon {
		public PresetsSection () {
			Direction = FillDirection.Vertical;
			AutoSizeAxes = Axes.Y;
			RelativeSizeAxes = Axes.X;
		}

		public string DisplayName => "Presets";

		public Drawable CreateIcon () => new SpriteIcon {
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
					lastPreset = new( config, XrConfigManager.TypeLookpuPreset );
					x.preset.Load( config, XrConfigManager.TypeLookpuPreset );
				}
			} ).Append( new SettingsButton {
				Text = "Previous",
				Action = () => {
					lastPreset?.Load( config, XrConfigManager.TypeLookpuPreset );
				}
			} )
#if DEBUG
			.Append( new SettingsButton {
				Text = "Print Current (Runtime Logs)",
				Action = () => {
					var preset = new SettingsPreset<XrConfigSetting>( config, XrConfigManager.TypeLookpuPreset );
					foreach ( var (k,v) in preset.values ) {
						Logger.Log( $"{k}: {v}" );
					}
				}
			} )
#endif
			.ToArray();
		}
	}
}
