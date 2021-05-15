using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Input.Custom {
	public abstract class CustomInput : Component {
		new public abstract string Name { get; }
		private Drawable settingDrawable;
		public Drawable SettingDrawable => settingDrawable ??= CreateSettingDrawable();

		[Resolved]
		protected List<object> rulesetActions { get; private set; }
		[Resolved]
		protected OsuGameXr game { get; private set; }

		protected virtual Drawable CreateSettingDrawable () {
			var x = new OsuTextFlowContainer {
				Anchor = Anchor.Centre,
				Origin = Anchor.Centre,
				AutoSizeAxes = Axes.Both,
				TextAnchor = Anchor.Centre
			};

			x.AddText( Name, x => x.Font = OsuFont.GetFont( weight: FontWeight.Bold ) );
			x.AddText( $" {((Name.EndsWith('s'))?"are":"is")} not configurable" );

			return x;
		}
	}
}
