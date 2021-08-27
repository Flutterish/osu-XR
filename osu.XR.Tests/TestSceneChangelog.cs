using osu.Framework.Graphics;
using osu.Game.Tests.Visual;
using osu.XR.Drawables;
using osuTK;

namespace osu.XR.Tests {
	public class TestSceneChangelog : OsuTestScene {
		protected override void LoadComplete () {
			base.LoadComplete();

			Add( new DragableDrawable( new ChangelogDrawable {
				Size = new Vector2( 400, 500 ),
				Anchor = Anchor.Centre,
				Origin = Anchor.Centre
			} ) );
		}
	}
}
