using osu.XR.Graphics.Panels.Settings;

namespace osu.XR.Graphics.Panels.Menu.Settings;

public partial class PresetSettingPanel : SettingsPanel {
	new public PresetPreview Content = null!;
	protected override SectionsContainer CreateSectionsContainer ()
		=> Content = new PresetPreview ( showSidebar: false );
}
