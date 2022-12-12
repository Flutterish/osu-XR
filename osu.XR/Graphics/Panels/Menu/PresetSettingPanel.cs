using osu.XR.Graphics.Settings;

namespace osu.XR.Graphics.Panels.Menu;

public partial class PresetSettingPanel : SettingsPanel {
	new public PresetPreview Content = null!;
	protected override SectionsContainer CreateSectionsContainer ()
		=> Content = new PresetPreview ( showSidebar: false );
}
