using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.XR.Components;
using osu.XR.Components.Skyboxes;
using osu.XR.Editor;
using osu.XR.Panels.Drawables;
using osu.XR.Panels.Overlays;

namespace osu.XR.Panels {
	public class SceneManagerPanel : HandheldPanel {
		SceneManagerDrawable content;

		protected override Drawable CreateContent () {
			return new PanelOverlayContainer {
				Child = content = new() {
					RelativeSizeAxes = Axes.Both
				}
			};
		}

		protected override void LoadComplete () {
			base.LoadComplete();
			content.SceneContainer = new SceneContainer();
			Root.Add( content.SceneContainer );

			var skybox = new SkyBox();
			var floorgrid = new FloorGrid();
			var dust = new DustEmitter();

			content.SceneContainer.Add( skybox );
			content.SceneContainer.Add( floorgrid );
			content.SceneContainer.Add( dust );
		}

		public override string DisplayName => "Scene";
		public override Drawable CreateIcon ()
			=> new SpriteIcon { Icon = FontAwesome.Solid.Image };
	}
}
