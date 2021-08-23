using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Framework.XR.GameHosts;
using osu.Game.Overlays.Settings;
using osu.XR.Drawables;

namespace osu.XR.Settings.Sections {
	public class InputSettingSection : SettingsSection {
		public override string DisplayName => "Input";
		public override Drawable CreateIcon () => new SpriteIcon {
			Icon = FontAwesome.Solid.Keyboard
		};

		[BackgroundDependencyLoader]
		private void load ( XrConfigManager config, GameHost host ) {
			Children = new Drawable[] {
				new SettingsEnumDropdown<InputMode> { LabelText = "Input mode", Current = config.GetBindable<InputMode>( XrConfigSetting.InputMode ) },
				// TODO these settings should only be shown when in appropriate input modes
				new SettingsCheckbox { LabelText = "Emulate touch with single pointer", Current = config.GetBindable<bool>( XrConfigSetting.SinglePointerTouch ), TooltipText = "Act as if a single pointer is a touch source.\nTwo pointers already behave this way." },
				new SettingsCheckbox { LabelText = "Tap only on press", Current = config.GetBindable<bool>( XrConfigSetting.TapOnPress ), TooltipText = "In touchscreen mode, press a button to tap the screen" },
				new SettingsSlider<int, PxSliderBar> { LabelText = "Deadzone", Current = config.GetBindable<int>( XrConfigSetting.Deadzone ), TooltipText = "Pointer deadzone after touching the screen or pressing a button" },
				new SettingsSlider<float, MetersSliderBar> { LabelText = "Player Height Offset", Current = (host as ExtendedRealityGameHost).PlayerHeightOffsetBindable, TooltipText = "Some devices might not send OXR a correct viewspace.\nUse this to correct this behaviour by offsetting your height." },
				new SettingsEnumDropdown<Hand> { LabelText = "Dominant hand", Current = config.GetBindable<Hand>( XrConfigSetting.DominantHand ) },
				new SettingsCheckbox { LabelText = "Disable teleporting", Current = config.GetBindable<bool>( XrConfigSetting.DisableTeleport ) },
			};
		}

	}
}
