using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.XR.Configuration;
using osu.XR.Graphics.Settings;

namespace osu.XR.Graphics.Panels.Settings;

public partial class InputSettingSection : SettingsSection {
	public override LocalisableString Header => "Input";
	public override Drawable CreateIcon () => new SpriteIcon {
		Icon = FontAwesome.Solid.Keyboard
	};

	[BackgroundDependencyLoader]
	private void load ( OsuXrConfigManager config ) {
		SettingsEnumDropdown<InputMode> inputMode;

		Children = new Drawable[] {
			(inputMode = new SettingsEnumDropdown<InputMode> { 
				LabelText = "Input mode", 
				TooltipText = "How your controllers interact with the panels" 
			}).PresetComponent( config, OsuXrSetting.InputMode ),
			new SettingsCheckbox {
				LabelText = "[Pointer] Emulate touch",
				TooltipText = "Emulate touch instead of mouse"
			}.PresetComponent( config, OsuXrSetting.TouchPointers ),
			new SettingsCheckbox {
				LabelText = "[Touch] Allow strumming",
				TooltipText = "Tap an additional time when releasing a button"
			}.PresetComponent( config, OsuXrSetting.TapStrum ),
			new SettingsSlider<int, PxSliderBar> { 
				LabelText = "Touch deadzone", 
				TooltipText = "Deadzone after interacting with a panel"
			}.PresetComponent( config, OsuXrSetting.Deadzone ),
			new SettingsEnumDropdown<HandSetting> { 
				LabelText = "Dominant hand"
			}.PresetComponent( config, OsuXrSetting.DominantHand ),
			new SettingsCheckbox { 
				LabelText = "Disable teleporting"
			}.PresetComponent( config, OsuXrSetting.DisableTeleport ),
		};
	}
}
