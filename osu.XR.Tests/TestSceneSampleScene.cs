using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics;
using osu.Game.Overlays.Settings;
using osu.XR.Drawables.Containers;
using osu.XR.Inspector;
using osuTK;
using System.Collections.Generic;

namespace osu.XR.Tests {
	public class TestSceneSampleScene : OsuTestScene3D {
		protected override void LoadComplete () {
			base.LoadComplete();

			Scene.Add( new TestComponent() );
		}
	}
}
