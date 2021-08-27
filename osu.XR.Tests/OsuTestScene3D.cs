using osu.Framework.Graphics;
using osu.Framework.XR.Components;
using osu.Framework.XR.Testing;
using osu.Game.Tests.Visual;

namespace osu.XR.Tests {
	public abstract class OsuTestScene3D : OsuTestScene {
		protected readonly Scene Scene;

		public OsuTestScene3D () {
			Add( Scene = new TestingScene() );
		}

		public override void Add ( Drawable drawable ) {
			if ( drawable is Drawable3D d3 )
				Scene.Add( d3 );
			else
				base.Add( drawable );
		}
	}
}
