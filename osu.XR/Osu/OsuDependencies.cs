using osu.Game;
using osu.Game.Rulesets;

namespace osu.XR.Osu;

public class OsuDependencies {
	public readonly Bindable<OsuGame?> OsuGame = new();
	BindableWithCurrent<RulesetInfo?> currentRuleset = new();
	public Bindable<RulesetInfo?> Ruleset => currentRuleset;

	public OsuDependencies () {
		OsuGame.BindValueChanged( v => {
			if ( v.NewValue is OsuGame game ) {
				if ( game.IsLoaded )
					onOsuLoaded( game );
				else
					game.OnLoadComplete += _ => onOsuLoaded( game );
			}
		} );
	}

	private void onOsuLoaded ( OsuGame game ) {
		if ( game != OsuGame.Value )
			return;

		var deps = game.Dependencies;
		currentRuleset.Current = deps.Get<Bindable<RulesetInfo?>>();
	}
}
