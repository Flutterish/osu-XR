using osu.Framework.Graphics;
using osu.Framework.Testing;

namespace osu.XR.Tests {
	public class TestSceneFormatedText : TestScene {
		public TestSceneFormatedText () {
			Add( new SceneWithMirrorWarning() {
				RelativeSizeAxes = Axes.Both
			} );
		}
	}
}
