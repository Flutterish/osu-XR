using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.XR.Components.Groups;
using osu.XR.Drawables;
using osu.XR.Panels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components.Panels {
	public class ChangelogPanel : HandheldPanel<ChangelogDrawable> {
		protected override ChangelogDrawable CreateContent ()
			=> new();

		public override string DisplayName => "Changelog";
		public override Drawable CreateIcon ()
			=> new SpriteIcon { Icon = FontAwesome.Solid.ClipboardList };
	}
}
