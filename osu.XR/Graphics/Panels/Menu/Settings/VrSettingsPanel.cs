using osu.Game.Overlays.Settings;
using osu.XR.Configuration;
using osu.XR.Graphics.Settings;

namespace osu.XR.Graphics.Panels.Settings;

public class VrSettingsPanel : SettingsPanel {
	protected override Sections CreateSectionsContainer ()
		=> new Sections( false );

	public class Sections : SectionsContainer {
		[Cached]
		SettingPresetContainer<OsuXrSetting> presetContainer = new();

		public Sections ( bool showSidebar ) : base( showSidebar ) { }

		protected override IEnumerable<SettingsSection> CreateSections () {
			yield return new InputSettingSection();
			yield return new GraphicsSettingSection();
			yield return new PresetsSettingSection();
		}

		protected override Drawable CreateHeader ()
			=> new SettingsHeader( "VR Settings", "change the way osu!XR behaves" );
	}
}