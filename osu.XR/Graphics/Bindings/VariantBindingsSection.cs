using osu.Framework.XR.VirtualReality;
using osu.Game.Rulesets;
using osu.XR.Graphics.Containers;
using osu.XR.Input;
using osu.XR.Input.Actions;

namespace osu.XR.Graphics.Bindings;

public partial class VariantBindingsSection : FillFlowContainer {
	public readonly Ruleset Ruleset;
	public readonly int Variant;

	public VariantBindingsSection ( Ruleset ruleset, int variant ) {
		Direction = FillDirection.Vertical;
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;

		Ruleset = ruleset;
		Variant = variant;
		RulesetActions = ruleset.GetDefaultKeyBindings( variant ).Select( x => x.Action ).Distinct().ToList();

		foreach ( var i in availableBindings ) {
			Add( new CollapsibleSection {
				Header = i.Name,
				Child = i.CreateEditor()
			} );
		}
	}

	[Cached(name: "RulesetActions" )]
	public readonly List<object> RulesetActions;

	List<ActionBinding> availableBindings = new() {
		new ButtonBinding( Hand.Left ),
		new ButtonBinding( Hand.Right ),
		new JoystickBindings( Hand.Left ),
		new JoystickBindings( Hand.Right ),
		new ClapBinding()
	};
}
