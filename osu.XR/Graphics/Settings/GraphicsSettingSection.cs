using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.XR.Configuration;

namespace osu.XR.Graphics.Settings;

public partial class GraphicsSettingSection : SettingsSection {
	public override LocalisableString Header => Localisation.Config.GraphicsStrings.Header;
	public override Drawable CreateIcon () => new SpriteIcon {
		Icon = FontAwesome.Solid.Laptop
	};

	[BackgroundDependencyLoader]
	private void load ( OsuXrConfigManager config ) {
		Children = new Drawable[] {
			new SettingsSlider<float,RadToDegreeSliderBar> {
				LabelText = Localisation.Config.Graphics.ScreenStrings.Arc
			}.PresetComponent( config, OsuXrSetting.ScreenArc ),
			new SettingsSlider<float,MetersSliderBar> { 
				LabelText = Localisation.Config.Graphics.ScreenStrings.Radius
			}.PresetComponent( config, OsuXrSetting.ScreenRadius ),
			new SettingsSlider<float,MetersSliderBar> { 
				LabelText = Localisation.Config.Graphics.ScreenStrings.Height
			}.PresetComponent( config, OsuXrSetting.ScreenHeight ),

			new SettingsSlider<int,PxSliderBar> { 
				LabelText = Localisation.Config.Graphics.ScreenStrings.ResolutionX
			}.PresetComponent( config, OsuXrSetting.ScreenResolutionX ),
			new SettingsSlider<int,PxSliderBar> {
				LabelText = Localisation.Config.Graphics.ScreenStrings.ResolutionY
			}.PresetComponent( config, OsuXrSetting.ScreenResolutionY ),

			new SettingsEnumDropdown<FeetSymbols> { 
				LabelText = Localisation.Config.Graphics.ShadowStrings.Label
			}.PresetComponent( config, OsuXrSetting.ShadowType )
		};
		// TODO computer interaction - render to screen, either vr view or custom camera
	}
}
