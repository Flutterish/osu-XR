using osu.XR.Graphics.Panels.Settings;

namespace osu.XR.Tests.Visual.Drawables;

public class TestSceneVrSettings : OsuTestScene {
	public TestSceneVrSettings () {
		Add( new VrSettingsPanel.Sections( false ) );
	}
}
