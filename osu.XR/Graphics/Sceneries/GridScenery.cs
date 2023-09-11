using osu.XR.Graphics.Sceneries.Components;

namespace osu.XR.Graphics.Sceneries;

public static class GridScenery {
	public static IEnumerable<ISceneryComponent> CreateComponents () => new ISceneryComponent[] {
		new VerticalGradientSkyBox(),
		new FloorGrid(),
		new BeatingCubes()
	};
}
