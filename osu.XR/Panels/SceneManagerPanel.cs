using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.XR.Editor;
using osu.XR.Panels.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Panels {
	public class SceneManagerPanel : HandheldPanel<SceneManagerDrawable> {
		protected override SceneManagerDrawable CreateContent ()
			=> new();

		protected override void LoadComplete () {
			base.LoadComplete();
			Content.SceneContainer = new SceneContainer();
			Root.Add( Content.SceneContainer );
		}

		public override string DisplayName => "Scene";
		public override Drawable CreateIcon ()
			=> new SpriteIcon { Icon = FontAwesome.Solid.Image };
	}
}
