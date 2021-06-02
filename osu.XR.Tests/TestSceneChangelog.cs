using osu.Framework.Graphics;
using osu.Game.Tests.Visual;
using osu.XR.Drawables;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Tests {
	public class TestSceneChangelog : OsuTestScene {
		protected override void LoadComplete () {
			base.LoadComplete();

			Add( new ChangelogPanel {
				Size = new Vector2( 400, 500 ),
				Anchor = Anchor.Centre,
				Origin = Anchor.Centre
			} );
		}
	}
}
