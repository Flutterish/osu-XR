using osu.XR.Graphics.Sceneries.Components;

namespace osu.XR.Graphics.Sceneries;

public static class LightsOutScenery {
	public static IEnumerable<ISceneryComponent> CreateComponents () => new ISceneryComponent[] {
		new RaveCylinder()
	};
}
