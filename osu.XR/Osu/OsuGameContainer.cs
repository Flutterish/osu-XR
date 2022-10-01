using osu.Framework.Platform;
using osu.Game;

namespace osu.XR.Osu;

/// <summary>
/// A container for <see cref="OsuGame"/> capable of capturing cached dependencies and reloading the game
/// </summary>
public class OsuGameContainer : CompositeDrawable {
	public readonly OsuDependencies OsuDependencies = new();

	[BackgroundDependencyLoader]
	private void load ( GameHost host ) {
		RelativeSizeAxes = Axes.Both;
		var osu = new OsuGame();
		OsuDependencies.OsuGame.Value = osu;

		osu.SetHost( host );
		AddInternal( osu );
	}
}
