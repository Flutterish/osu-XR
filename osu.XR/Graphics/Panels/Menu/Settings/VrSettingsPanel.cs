using osu.Game.Overlays.Settings;
using osu.XR.Configuration;
using osu.XR.Graphics.Panels.Menu.Settings;
using osu.XR.Graphics.Settings;

namespace osu.XR.Graphics.Panels.Settings;

public partial class VrSettingsPanel : SettingsPanel {
	public PresetSettingPanel PresetSettings { get; private set; } = null!;

	Sections sections = null!;
	protected override Sections CreateSectionsContainer () {
		sections = new Sections( false );
		PresetSettings = new();
		var presets = PresetSettings.Content;

		presets.PresetContainer.Presets.BindTo( sections.PresetContainer.Presets );
		presets.PresetContainer.IsEditingBindable.BindTo( sections.PresetContainer.IsEditingBindable );
		presets.PresetContainer.SelectedPresetBindable.BindTo( sections.PresetContainer.SelectedPresetBindable );

		presets.PresetContainer.SelectedPresetBindable.BindValueChanged( v => {
			PresetSettings.FadeTo( v.NewValue != null ? 1 : 0, 200, Easing.Out );
			onVisibilityChanged();
		}, true );

		VisibilityChanged += (_, _) => onVisibilityChanged();
		FinishTransforms( true );

		return sections;
	}

	void onVisibilityChanged () {
		PresetSettings.IsColliderEnabled = IsRendered && PresetSettings.Content.PresetContainer.SelectedPresetBindable.Value != null;
	}

	[BackgroundDependencyLoader(true)]
	private void load ( OsuXrConfigManager? config ) {
		if ( config == null )
			return;

		sections.PresetContainer.Presets.BindTo( config.Presets );
		sections.PresetContainer.SelectedPresetBindable.BindValueChanged( v => {
			config.ApplyPresetPreview( v.NewValue );
		} );
	}

	public partial class Sections : SectionsContainer {
		[Cached]
		public readonly SettingPresetContainer<OsuXrSetting> PresetContainer = new();

		public Sections ( bool showSidebar ) : base( showSidebar ) { }

		public readonly PresetsSettingSection Presets = new();

		protected override IEnumerable<SettingsSection> CreateSections () {
			yield return new InputSettingSection();
			yield return new GraphicsSettingSection();
			yield return Presets;
		}

		protected override Drawable CreateHeader ()
			=> new SettingsHeader( "VR Settings", "change the way osu!XR behaves" );
	}
}