using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.XR.Configuration;

namespace osu.XR.Graphics.Settings;

public partial class InputSettingSection : SettingsSection {
	public override LocalisableString Header => Localisation.Config.InputStrings.Header;
	public override Drawable CreateIcon () => new SpriteIcon {
		Icon = FontAwesome.Solid.Keyboard
	};

	[BackgroundDependencyLoader]
	private void load ( OsuXrConfigManager config ) {
		Children = new Drawable[] {
			new SettingsEnumDropdown<InputMode> { 
				LabelText = Localisation.Config.Input.ModeStrings.Label, 
				TooltipText = Localisation.Config.Input.ModeStrings.Tooltip
			}.PresetComponent( config, OsuXrSetting.InputMode ),
			new SettingsCheckbox {
				LabelText = Localisation.Config.Input.PointerTouchStrings.Label,
				TooltipText = Localisation.Config.Input.PointerTouchStrings.Tooltip
			}.PresetComponent( config, OsuXrSetting.TouchPointers ),
			new SettingsCheckbox {
				LabelText = Localisation.Config.Input.TapStrumStrings.Label,
				TooltipText = Localisation.Config.Input.TapStrumStrings.Tooltip
			}.PresetComponent( config, OsuXrSetting.TapStrum ),
			new SettingsSlider<int, PxSliderBar> { 
				LabelText = Localisation.Config.Input.TouchDeadzoneStrings.Label, 
				TooltipText = Localisation.Config.Input.TouchDeadzoneStrings.Tooltip
			}.PresetComponent( config, OsuXrSetting.Deadzone ), // TODO touch deadzone
			new SettingsEnumDropdown<MotionRange> {
				LabelText = @"Hand skeletion motion range"
			}.PresetComponent( config, OsuXrSetting.HandSkeletonMotionRange ),
			new SettingsEnumDropdown<HandSetting> {
				LabelText = Localisation.Config.Input.MainHandStrings.Label
			}.PresetComponent( config, OsuXrSetting.DominantHand ),
			new SettingsCheckbox { 
				LabelText = Localisation.Config.Input.DisableTeleportingStrings.Label,
				TooltipText = Localisation.Config.Input.DisableTeleportingStrings.Tooltip
			}.PresetComponent( config, OsuXrSetting.DisableTeleport ),
		};
	}
}
