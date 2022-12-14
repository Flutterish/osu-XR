using osu.Framework.Input.Bindings;
using osu.Framework.Input;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;

namespace osu.XR.Osu;

#nullable disable
public record PlayerInfo {
	public Player Player { get; init; }
	public DrawableRuleset DrawableRuleset { get; init; }
	public PassThroughInputManager InputManager { get; init; }
	public Type RulesetActionType { get; init; }
	public KeyBindingContainer KeyBindingContainer { get; init; }
	public Bindable<IReadOnlyList<Mod>> Mods { get; init; }
	public int Variant { get; init; }
}
