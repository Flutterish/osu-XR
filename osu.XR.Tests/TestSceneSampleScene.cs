using osu.Framework.XR.Components;

namespace osu.XR.Tests {
	public class TestSceneSampleScene : OsuTestScene3D {
		protected override void LoadComplete () {
			base.LoadComplete();

			Scene.Add( new TestComponent() );
		}
	}
}
