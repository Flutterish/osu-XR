using osu.Game;

namespace osu.XR.Osu;

public class OsuDependencies {
	public readonly Bindable<OsuGame?> OsuGame = new();
}
