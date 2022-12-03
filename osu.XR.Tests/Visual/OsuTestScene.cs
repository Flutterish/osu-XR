using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.XR.Graphics.Panels;
using osu.Game;
using osu.Game.Graphics.Cursor;
using osu.XR.Osu;

namespace osu.XR.Tests.Visual;

public abstract partial class OsuTestScene : TestScene {
	OsuDependencies dependencies = new();
	VirtualGameHost virtualGameHost = null!;
	OsuGameBase osu = new OsuGameBase { Size = osuTK.Vector2.Zero };

	OsuDepsContainer deps;
	protected override Container<Drawable> Content => deps;
	public OsuTestScene () {
		dependencies.OsuGameBase.Value = osu;
		base.Content.Add( deps = new( dependencies, this ) { RelativeSizeAxes = Axes.Both } );
	}

	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		virtualGameHost = new( parent.Get<GameHost>() );
		osu.SetHost( virtualGameHost );
		return base.CreateChildDependencies( parent );
	}

	partial class OsuDepsContainer : Container {
		OsuDependencies dependencies;
		OsuTestScene scene;

		OsuTooltipContainer tooltips = new(null) { RelativeSizeAxes = Axes.Both };
		OsuContextMenuContainer contextMenu = new() { RelativeSizeAxes = Axes.Both };
		protected override Container<Drawable> Content => contextMenu;

		public OsuDepsContainer ( OsuDependencies dependencies, OsuTestScene scene ) {
			this.dependencies = dependencies;
			this.scene = scene;
			AddInternal( tooltips );
			tooltips.Add( contextMenu );
		}

		protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
			scene.LoadComponent( scene.osu );
			return base.CreateChildDependencies( dependencies );
		}
	}
}
