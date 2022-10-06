using osu.Game.Overlays.Settings;
using osu.XR.Configuration;
using osu.XR.Graphics.Panels.Settings;
using osu.XR.Graphics.Settings;

namespace osu.XR.Graphics.Panels.Menu.Settings;

public class PresetPreview : SettingsPanel.SectionsContainer {
	[Cached]
	public readonly SettingPresetContainer<OsuXrSetting> PresetContainer = new();

	public readonly Bindable<ConfigurationPreset<OsuXrSetting>?> PresetBindable = new();
	public ConfigurationPreset<OsuXrSetting>? Preset {
		get => PresetBindable.Value;
		set => PresetBindable.Value = value;
	}

	public PresetPreview ( bool showSidebar ) : base( showSidebar ) {
		PresetContainer.SelectedPresetBindable.BindTo( PresetBindable );
		PresetContainer.IsPreviewBindable.Value = true;
	}

	protected override IEnumerable<SettingsSection> CreateSections () {
		yield return new InputSettingSection();
		yield return new GraphicsSettingSection();
	}

	protected override Drawable CreateHeader ()
		=> new SettingsHeader( "Preset Preview", "" );
}
