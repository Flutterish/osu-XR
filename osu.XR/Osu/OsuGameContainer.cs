using osu.Framework.Platform;
using osu.Framework.XR.Graphics.Panels;
using osu.Game;

namespace osu.XR.Osu;

/// <summary>
/// A container for <see cref="OsuGame"/> capable of capturing cached dependencies and reloading the game
/// </summary>
public class OsuGameContainer : CompositeDrawable {
	public readonly OsuDependencies OsuDependencies = new();
	public VirtualGameHost VirtualGameHost { get; private set; } = null!;

	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		var deps = new DependencyContainer( parent );
		VirtualGameHost = new( parent.Get<GameHost>() );
		deps.CacheAs<GameHost>( VirtualGameHost );
		return base.CreateChildDependencies( deps );
	}

	[BackgroundDependencyLoader]
	private void load () {
		RelativeSizeAxes = Axes.Both;
		var osu = new OsuGame();
		OsuDependencies.OsuGame.Value = osu;

		osu.SetHost( VirtualGameHost );
		AddInternal( osu );
	}
}
