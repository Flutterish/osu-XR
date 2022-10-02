using osu.Game;
using osu.Game.Rulesets;

namespace osu.XR.Osu;

public class OsuDependencies : IReadOnlyDependencyContainer {
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

	public object? Get ( Type type ) {
		return OsuGame.Value?.Dependencies.Get( type );
	}

	public object? Get ( Type type, CacheInfo info ) {
		return OsuGame.Value?.Dependencies.Get( type, info );
	}

	DependencyContainer? injector;
	public void Inject<T> ( T instance ) where T : class {
		(injector ??= new( this )).Inject( instance );
	}
}
