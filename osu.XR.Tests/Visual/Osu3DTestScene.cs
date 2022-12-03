using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.XR.Graphics.Panels;
using osu.Game;
using osu.XR.Osu;

namespace osu.XR.Tests.Visual;

// TODO wait for o!f DI SG fix (confict between member name and namespace)
#pragma warning disable OFSG001
public abstract class Osu3DTestScene : Basic3DTestScene {
	[Cached]
	OsuDependencies dependencies = new();
	VirtualGameHost virtualGameHost = null!;
	OsuGameBase osu = null!;

	public Osu3DTestScene () {
		osu = new OsuGameBase { Size = osuTK.Vector2.Zero };
	}

	public override void Add ( Drawable drawable ) {
		if ( IsLoaded )
			base.Add( drawable );
		else
			Schedule( () => base.Add( drawable ) );
	}

	[BackgroundDependencyLoader]
	private void load ( GameHost host ) {
		virtualGameHost = new( host );
		osu.SetHost( virtualGameHost );
		base.Add( new Container { Child = osu } );
		dependencies.OsuGameBase.Value = osu;
	}
}
