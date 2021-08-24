using osu.Framework.Graphics;
using osu.XR.Panels.Drawables;
using osu.XR.Panels.Overlays;
using osuTK;

namespace osu.XR.Tests {
	public class TestSceneOBJImport : OsuTestScene3D {
		SceneManagerDrawable scenePanel;

		protected override void LoadComplete () {
			base.LoadComplete();
			Add( new PanelOverlayContainer {
				Size = new Vector2( 400, 500 ) * 1.2f,
				Child = scenePanel = new SceneManagerDrawable {
					RelativeSizeAxes = Axes.Both,
					Anchor = Anchor.Centre,
					Origin = Anchor.Centre
				},
				Anchor = Anchor.Centre,
				Origin = Anchor.Centre
			} );

			scenePanel.SceneContainer = new Editor.SceneContainer();
		}
	}
}
