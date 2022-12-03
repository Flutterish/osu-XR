using osu.Game;
using osu.Game.Rulesets;

namespace osu.XR.Osu;

public class OsuDependencies : IReadOnlyDependencyContainer {
	public readonly Bindable<OsuGame?> OsuGame = new();
	public readonly Bindable<OsuGameBase?> OsuGameBase = new();
	BindableWithCurrent<RulesetInfo?> currentRuleset = new();
	public Bindable<RulesetInfo?> Ruleset => currentRuleset;

	enum UpdateSource {
		None,
		OsuGame,
		OsuGameBase
	}
	public OsuDependencies () {
		UpdateSource updateSource = UpdateSource.None;

		OsuGame.BindValueChanged( v => {
			if ( updateSource is UpdateSource.None ) {
				updateSource = UpdateSource.OsuGame;
				OsuGameBase.Value = v.NewValue;
				updateSource = UpdateSource.None;
			}
		} );

		OsuGameBase.BindValueChanged( v => {
			if ( updateSource is UpdateSource.None ) {
				updateSource = UpdateSource.OsuGameBase;
				if ( v.NewValue is OsuGame osu )
					OsuGame.Value = osu;
				else
					OsuGame.Value = null;
				updateSource = UpdateSource.None;
			}

			if ( v.NewValue is OsuGameBase game ) {
				if ( game.IsLoaded )
					onOsuLoaded( game );
				else
					game.OnLoadComplete += _ => onOsuLoaded( game );
			}
		} );
	}

	private void onOsuLoaded ( OsuGameBase game ) {
		if ( game != OsuGameBase.Value )
			return;

		var deps = game.Dependencies;
		currentRuleset.Current = deps.Get<Bindable<RulesetInfo?>>();
	}

	public object? Get ( Type type ) {
		return OsuGameBase.Value?.Dependencies.Get( type );
	}

	public object? Get ( Type type, CacheInfo info ) {
		return OsuGameBase.Value?.Dependencies.Get( type, info );
	}

	DependencyContainer? injector;
	public void Inject<T> ( T instance ) where T : class, IDependencyInjectionCandidate {
		(injector ??= new( this )).Inject( instance );
	}
}
