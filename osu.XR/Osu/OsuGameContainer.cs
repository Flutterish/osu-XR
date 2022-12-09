using osu.Framework.Platform;
using osu.Framework.XR.Graphics.Panels;
using osu.Game;

namespace osu.XR.Osu;

/// <summary>
/// A container for <see cref="OsuGame"/> capable of capturing cached dependencies and reloading the game
/// </summary>
public partial class OsuGameContainer : CompositeDrawable {
	public readonly OsuDependencies OsuDependencies = new();
	public VirtualGameHost VirtualGameHost { get; private set; } = null!;
	OsuGame osu = new();

	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		var deps = new DependencyContainer( parent );
		VirtualGameHost = new( parent.Get<GameHost>() );
		deps.CacheAs<GameHost>( VirtualGameHost );
		deps.CacheAs<Framework.Game>( osu );
		deps.CacheAs<OsuGameBase>( osu );
		deps.CacheAs<OsuGame>( osu );
		return base.CreateChildDependencies( deps );
	}

	[BackgroundDependencyLoader]
	private void load () {
		RelativeSizeAxes = Axes.Both;
		OsuDependencies.OsuGame.Value = osu;

		osu.SetHost( VirtualGameHost );
		AddInternal( osu );
	}
}
