using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
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
		}

		public override string DisplayName => "Scene";
		public override Drawable CreateIcon ()
			=> new SpriteIcon { Icon = FontAwesome.Solid.Image };
	}
}
