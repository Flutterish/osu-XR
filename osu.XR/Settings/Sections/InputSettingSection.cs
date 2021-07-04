using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Framework.XR.GameHosts;
using osu.Game.Overlays.Settings;
using osu.XR.Components.Groups;
using osu.XR.Drawables;

namespace osu.XR.Settings.Sections {
	public class InputSettingSection : FillFlowContainer, IHasName, IHasIcon {
		public InputSettingSection () {
			Direction = FillDirection.Vertical;
			AutoSizeAxes = Axes.Y;
			RelativeSizeAxes = Axes.X;
		}

		public Drawable CreateIcon () => new SpriteIcon {
			Icon = FontAwesome.Solid.Keyboard
		};

		[BackgroundDependencyLoader]
		private void load ( XrConfigManager config, GameHost host ) {
			Children = new Drawable[] {
				new SettingsEnumDropdown<InputMode> { LabelText = "Input mode", Current = config.GetBindable<InputMode>( XrConfigSetting.InputMode ) },
				new SettingsCheckbox { LabelText = "Emulate touch with single pointer", Current = config.GetBindable<bool>( XrConfigSetting.SinglePointerTouch ), TooltipText = "In single pointer mode, send position only when holding a button" },
				new SettingsCheckbox { LabelText = "Tap only on press", Current = config.GetBindable<bool>( XrConfigSetting.TapOnPress ), TooltipText = "In touchscreen mode, hold a button to touch the screen" },
				new SettingsSlider<int, PxSliderBar> { LabelText = "Deadzone", Current = config.GetBindable<int>( XrConfigSetting.Deadzone ), TooltipText = "Pointer deadzone after touching the screen or pressing a button" },
				new SettingsSlider<float> { LabelText = "Player Height Offset", Current = (host as ExtendedRealityGameHost).PlayerHeightOffset },
				new SettingsEnumDropdown<Hand> { LabelText = "Dominant hand", Current = config.GetBindable<Hand>( XrConfigSetting.DominantHand ) }
			};
		}

		public string DisplayName => "Input";
	}
}
