using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using System.Reflection;

namespace osu.XR.Osu;

public class PlayerInfo {
	public required Player Player { get; init; }
	public required DrawableRuleset DrawableRuleset { get; init; }
	public required PassThroughInputManager InputManager { get; init; }
	public required Type RulesetActionType { get; init; }
	public required KeyBindingContainer KeyBindingContainer { get; init; }
	public IReadOnlyList<Mod> Mods { get; }
	public required int Variant { get; init; }

	public bool IsPaused => DrawableRuleset.IsPaused.Value;

	Dictionary<Type, Mod> mods = new();
	public PlayerInfo ( IReadOnlyList<Mod> mods ) {
		Mods = mods;

		updateActions.Clear();
		foreach ( var i in mods ) {
			this.mods.Add( i.GetType(), i );
		}

		if ( getMod<ModNoScope>() is ModNoScope noScope ) {
			var type = typeof( ModNoScope );
			var opacityField = type.GetField( "ComboBasedAlpha", BindingFlags.Instance | BindingFlags.NonPublic )!;
			updateActions.Add( () => {
				noScopeOpacity = (float)opacityField.GetValue( noScope )!;
			} );
		}
	}

	T? getMod<T> () where T : Mod {
		return Mods.OfType<T>().FirstOrDefault();
	}

	float noScopeOpacity = 1f;
	public float CursorOpacityFromMods => IsPaused ? 1f : noScopeOpacity;

	List<Action> updateActions = new();
	public void Update () {
		foreach ( var i in updateActions ) {
			i();
		}
	}
}
