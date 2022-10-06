using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.XR.Configuration;
using osu.XR.Graphics.Settings;

namespace osu.XR.Graphics.Panels.Settings;

public class InputSettingSection : SettingsSection {
	public override LocalisableString Header => "Input";
	public override Drawable CreateIcon () => new SpriteIcon {
		Icon = FontAwesome.Solid.Keyboard
	};

	[BackgroundDependencyLoader]
	private void load ( OsuXrConfigManager config ) {
		Children = new Drawable[] {
			new SettingsEnumDropdown<InputMode> { 
				LabelText = "Input mode", 
				TooltipText = "How your controllers interact with the panels" 
			}.PresetComponent( config, OsuXrSetting.InputMode ),
			new ConditionalSettingsContainer<InputMode> {
				Current = config.GetBindable<InputMode>( OsuXrSetting.InputMode ),
				[InputMode.SinglePointer] = new Drawable[] {
					new SettingsCheckbox { 
						LabelText = "[Single pointer] Emulate touch", 
						TooltipText = "Emulate touch instead of mouse" 
					}.PresetComponent( config, OsuXrSetting.SinglePointerTouch ),
				},
				[InputMode.TouchScreen] = new Drawable[] {
					new SettingsCheckbox { 
						LabelText = "[Touchscreen] Tap only on press", 
						TooltipText = "Press a button to tap the screen" 
					}.PresetComponent( config, OsuXrSetting.TapOnPress )
				}
			},
			new SettingsSlider<int, PxSliderBar> { 
				LabelText = "Touch deadzone", 
				TooltipText = "Deadzone after interacting with a panel"
			}.PresetComponent( config, OsuXrSetting.Deadzone ),
			new SettingsEnumDropdown<Hand> { 
				LabelText = "Dominant hand"
			}.PresetComponent( config, OsuXrSetting.DominantHand ),
			new SettingsCheckbox { 
				LabelText = "Disable teleporting"
			}.PresetComponent( config, OsuXrSetting.DisableTeleport ),
		};
	}
}
