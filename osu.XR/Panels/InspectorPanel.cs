using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.XR.Inspector;
using osu.XR.Panels;

namespace osu.XR.Components.Panels {
	public class InspectorPanel : HandheldPanel<InspectorDrawable> {
		protected override InspectorDrawable CreateContent ()
			=> new();

		public override string DisplayName => "Inspector";
		public override Drawable CreateIcon ()
			=> new SpriteIcon { Icon = FontAwesome.Solid.Search };
	}
}
