using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.XR.Graphics.Panels;
using osu.Framework.XR.Testing.VirtualReality;
using osu.Game;
using osu.Game.Graphics.Cursor;
using osu.XR.Osu;

namespace osu.XR.Tests.Visual;

public abstract partial class OsuTestScene : TestScene {
	[Cached]
	protected readonly OsuDependencies OsuDependencies = new();
	VirtualGameHost virtualGameHost = null!;
	OsuGameBase osu = new OsuGameBase { Size = osuTK.Vector2.Zero };

	OsuDepsContainer deps;
	protected override Container<Drawable> Content => deps;
	public OsuTestScene () {
		OsuDependencies.OsuGameBase.Value = osu;
		base.Content.Add( deps = new( OsuDependencies, this ) { RelativeSizeAxes = Axes.Both } );
	}

	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		virtualGameHost = new( parent.Get<GameHost>() );
		osu.SetHost( virtualGameHost );
		return base.CreateChildDependencies( parent );
	}

	protected void AddVrControls ( VirtualVrInput vr ) {
		var controls = vr.CreateControlsDrawable();
		controls.AutoSizeAxes = Axes.Y;
		controls.RelativeSizeAxes = Axes.X;
		Add( new Container {
			Depth = -1,
			RelativeSizeAxes = Axes.Both,
			Size = new( 0.4f, 0.5f ),
			Origin = Anchor.BottomRight,
			Anchor = Anchor.BottomRight,
			Children = new Drawable[] {
				new Box { Colour = FrameworkColour.GreenDark, RelativeSizeAxes = Axes.Both },
				new BasicScrollContainer {
					RelativeSizeAxes = Axes.Both,
					Padding = new MarginPadding( 16 ),
					ScrollbarVisible = false,
					Child = controls
				}
			}
		} );
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
