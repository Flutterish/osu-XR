using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.XR.Drawables;
using osu.XR.Panels;

namespace osu.XR.Components.Panels {
	public class ChangelogPanel : HandheldPanel<ChangelogDrawable> {
		protected override ChangelogDrawable CreateContent ()
			=> new();

		public override string DisplayName => "Changelog";
		public override Drawable CreateIcon ()
			=> new SpriteIcon { Icon = FontAwesome.Solid.ClipboardList };
	}
}
