using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.XR.Components.Groups;
using osu.XR.Drawables;
using osu.XR.Panels;

namespace osu.XR.Components.Panels {
	public class ConfigPanel : HandheldPanel<VRConfigDrawable> {
		protected override VRConfigDrawable CreateContent ()
			=> new();

		public override string DisplayName => "Settings";
		public override Drawable CreateIcon ()
			=> new SpriteIcon { Icon = FontAwesome.Solid.Cog };
	}
}
