using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.XR.Configuration;
using osu.XR.Graphics.Settings;

namespace osu.XR.Graphics.Panels.Settings;

public class GraphicsSettingSection : SettingsSection {
	public override LocalisableString Header => "Graphics";
	public override Drawable CreateIcon () => new SpriteIcon {
		Icon = FontAwesome.Solid.Laptop
	};

	[BackgroundDependencyLoader]
	private void load ( OsuXrConfigManager config ) {
		Children = new Drawable[] {
			new SettingsSlider<float,RadToDegreeSliderBar> {
				LabelText = "Screen arc"
			}.PresetComponent( config, OsuXrSetting.ScreenArc ),
			new SettingsSlider<float,MetersSliderBar> { 
				LabelText = "Screen radius"
			}.PresetComponent( config, OsuXrSetting.ScreenRadius ),
			new SettingsSlider<float,MetersSliderBar> { 
				LabelText = "Screen height"
			}.PresetComponent( config, OsuXrSetting.ScreenHeight ),

			new SettingsSlider<int,PxSliderBar> { 
				LabelText = "Screen resolution X"
			}.PresetComponent( config, OsuXrSetting.ScreenResolutionX ),
			new SettingsSlider<int,PxSliderBar> {
				LabelText = "Screen resolution Y"
			}.PresetComponent( config, OsuXrSetting.ScreenResolutionY ),

			new SettingsEnumDropdown<FeetSymbols> { 
				LabelText = "Shadow type"
			}.PresetComponent( config, OsuXrSetting.ShadowType )
		};
		// TODO computer interaction - render to screen, either vr view or custom camera
	}
}
