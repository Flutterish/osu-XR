using osu.Framework.Graphics;
using osu.Framework.XR.Components;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.XR.Tests {
	public abstract class OsuTestScene3D : OsuTestScene {
		protected Scene Scene;
		public OsuTestScene3D () {
			Scene = new Scene {
				RelativeSizeAxes = Axes.Both,
				Camera = new() { Position = new Vector3( 1, 1, -3 ) }
			};
			Scene.Add( Scene.Camera );
			Add( Scene );
		}

		protected override void LoadComplete () {
			base.LoadComplete();
			Scene.RenderToScreen = true;
		}
	}
}
