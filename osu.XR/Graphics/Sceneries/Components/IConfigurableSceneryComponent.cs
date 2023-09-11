using osu.Game.Overlays.Settings;

namespace osu.XR.Graphics.Sceneries.Components;

public interface IConfigurableSceneryComponent : ISceneryComponent {
	SettingsSection CreateSettings ();
}
