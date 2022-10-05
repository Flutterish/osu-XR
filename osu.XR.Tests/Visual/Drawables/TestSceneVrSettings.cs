using osu.XR.Graphics.Panels.Settings;

namespace osu.XR.Tests.Visual.Drawables;

public class TestSceneVrSettings : OsuTestScene {
	public TestSceneVrSettings () {
		VrSettingsPanel panel = new();
		VrSettingsPanel.Sections sections = new( false, panel );
		Add( sections );
	}
}
