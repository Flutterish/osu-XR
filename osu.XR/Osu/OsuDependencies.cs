using osu.Framework.Input.Bindings;
using osu.Framework.Input;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using System.Reflection;

namespace osu.XR.Osu;

public class OsuDependencies : IReadOnlyDependencyContainer {
	public readonly Bindable<OsuGame?> OsuGame = new();
	public readonly Bindable<OsuGameBase?> OsuGameBase = new();
	BindableWithCurrent<RulesetInfo?> currentRuleset = new();
	public Bindable<RulesetInfo?> Ruleset => currentRuleset;

	BindableWithCurrent<PlayerInfo?> currentPlayer = new();
	public Bindable<PlayerInfo?> Player => currentPlayer;

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
			Player.Value = null;
			if ( screenStack != null ) {
				screenStack.ScreenPushed -= onOsuScreenPushed;
				screenStack.ScreenExited -= onOsuScreenExited;
				screenStack = null;
			}

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

	T GetReflected<T> ( object target, string? name = null ) {
		var prop = target.GetType().GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
			.Where( x => (name is null || x.Name == name) && x.CanRead && x.GetIndexParameters().Length == 0 && x.PropertyType.IsAssignableTo( typeof( T ) ) ).FirstOrDefault();

		if ( prop != null )
			return (T)prop.GetValue( target )!;

		var field = target.GetType().GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
			.Where( x => (name is null || x.Name == name) && x.FieldType.IsAssignableTo( typeof( T ) ) ).FirstOrDefault();

		if ( field != null )
			return (T)field.GetValue( target )!;

		return default!;
	}

	OsuScreenStack? screenStack;
	void onOsuScreenPushed ( IScreen lastScreen, IScreen newScreen ) {
		onOsuScreenChanged( newScreen );
	}
	private void onOsuScreenExited ( IScreen lastScreen, IScreen newScreen ) {
		onOsuScreenChanged( newScreen );
	}

	void onOsuScreenChanged ( IScreen screen ) {
		try {
			if ( screen is not Player player ) {
				Player.Value = null;
				return;
			}

			var drawableRuleset = GetReflected<DrawableRuleset>( player );
			var inputManager = GetReflected<PassThroughInputManager>( drawableRuleset );

			var managerType = inputManager.GetType();
			while ( !managerType.IsGenericType || managerType.GetGenericTypeDefinition() != typeof( RulesetInputManager<> ) ) {
				managerType = managerType.BaseType!;
			}

			var actionType = managerType.GetGenericArguments()[0];
			var bindings = GetReflected<KeyBindingContainer>( inputManager );

			Player.Value = new PlayerInfo {
				Player = player,
				DrawableRuleset = drawableRuleset,
				InputManager = inputManager,
				RulesetActionType = actionType,
				KeyBindingContainer = bindings,
				Mods = GetReflected<Bindable<IReadOnlyList<Mod>>>( player ),
				Variant = GetReflected<int>( drawableRuleset, nameof( DrawableRuleset<HitObject>.Variant ) )
			};
		}
		catch ( Exception e ) {
			ExceptionInvoked?.Invoke( @"Could not find the playfield. Please report this to the osu!xr developers", e );
		}
	}

	public event Action<LocalisableString, Exception>? ExceptionInvoked;

	private void onOsuLoaded ( OsuGameBase game ) {
		try {
			if ( game != OsuGameBase.Value )
				return;

			var deps = game.Dependencies;
			currentRuleset.Current = deps.Get<Bindable<RulesetInfo?>>();

			if ( game is not OsuGame osu )
				return;

			try {
				screenStack = GetReflected<OsuScreenStack>( osu );
				screenStack.ScreenPushed += onOsuScreenPushed;
				screenStack.ScreenExited += onOsuScreenExited;
			}
			catch ( Exception e ) {
				ExceptionInvoked?.Invoke( @"Could not find the osu! screen stack. Please report this to the osu!xr developers", e );
			}
		}
		finally {
			OsuLoaded?.Invoke( this );
		}
	}

	public event Action<OsuDependencies>? OsuLoaded;

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
