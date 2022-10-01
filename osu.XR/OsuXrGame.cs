using osu.Framework.XR.Graphics.Rendering;

namespace osu.XR;

public class OsuXrGame : OsuXrGameBase {
	Scene Scene;

	public OsuXrGame () {
		Add( Scene = new() {
			RelativeSizeAxes = Framework.Graphics.Axes.Both
		} );

		Scene.Camera.Z = -5;
	}
}
