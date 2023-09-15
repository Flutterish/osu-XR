using osu.Game.Overlays.Settings;
using osu.XR.Configuration;
using osu.XR.Configuration.Presets;
using osu.XR.Graphics.Settings;

namespace osu.XR.Graphics.Panels.Menu;

public partial class VrSettingsPanel : SettingsPanel {
	public PresetSettingPanel PresetSettings { get; private set; } = null!;

	Sections sections = null!;
	protected override Sections CreateSectionsContainer () {
		sections = new Sections( false );
		PresetSettings = new();
		var presets = PresetSettings.Content;

		presets.PresetContainer.Presets.BindTo( sections.PresetContainer.Presets );
		presets.PresetContainer.IsSlideoutEnabled.BindTo( sections.PresetContainer.IsSlideoutEnabled );
		presets.PresetContainer.SelectedPreset.BindTo( sections.PresetContainer.SelectedPreset );

		presets.PresetContainer.SelectedPreset.BindValueChanged( v => {
			PresetSettings.FadeTo( v.NewValue != null ? 1 : 0, 200, Easing.Out );
			onVisibilityChanged();
		}, true );

		VisibilityChanged += (_, _) => onVisibilityChanged();
		FinishTransforms( true );

		return sections;
	}

	void onVisibilityChanged () {
		PresetSettings.IsColliderEnabled = IsRendered && PresetSettings.Content.PresetContainer.SelectedPreset.Value != null;
	}

	public partial class Sections : SectionsContainer {
		[Cached]
		public readonly ConfigPresetSource<OsuXrSetting> PresetContainer = new( PresetViewType.ItemList, slideoutDirection: LeftRight.Left );

		public Sections ( bool showSidebar ) : base( showSidebar ) { }

		public readonly PresetsSettingSection Presets = new();

		[BackgroundDependencyLoader]
		private void load ( OsuXrConfigManager? config ) {
			if ( config == null )
				return;

			PresetContainer.Presets.BindTo( config.Presets );
			PresetContainer.SelectedPreset.BindValueChanged( v => {
				config.SetCurrentPreset( v.NewValue );
			} );
		}

		protected override IEnumerable<SettingsSection> CreateSections () {
			yield return new InputSettingSection();
			yield return new GraphicsSettingSection();
			yield return Presets;
		}

		protected override Drawable CreateHeader ()
			=> new SettingsHeader( Localisation.ConfigStrings.Header, Localisation.ConfigStrings.Flavour );
	}
}