using osu.Game.Overlays.Settings;
using osu.XR.Configuration;
using osu.XR.Graphics.Settings;

namespace osu.XR.Graphics.Panels.Settings;

public partial class VrSettingsPanel : SettingsPanel {
	Sections sections = null!;
	protected override Sections CreateSectionsContainer ()
		=> sections = new Sections ( false );

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