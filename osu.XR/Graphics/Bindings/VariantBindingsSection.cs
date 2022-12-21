using osu.Framework.XR.VirtualReality;
using osu.Game.Rulesets;
using osu.XR.Graphics.Containers;
using osu.XR.Input;
using osu.XR.Input.Actions;

namespace osu.XR.Graphics.Bindings;

public partial class VariantBindingsSection : FillFlowContainer {
	public readonly Ruleset Ruleset;
	public readonly int Variant;

	VariantBindings? _bindings;
	public VariantBindings Bindings {
		get => _bindings ??= new( Variant );
		init => _bindings = value;
	}

	public VariantBindingsSection ( Ruleset ruleset, int variant ) {
		Direction = FillDirection.Vertical;
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;

		Ruleset = ruleset;
		Variant = variant;
		RulesetActions = ruleset.GetDefaultKeyBindings( variant ).Select( x => x.Action ).Distinct().ToList();
	}

	protected override void LoadComplete () {
		base.LoadComplete();

		foreach ( var i in getAvailableBindings() ) {
			var binding = Bindings.GetOrAdd( i );
			if ( binding.CreateEditor() is not Drawable editor )
				continue;

			Add( new CollapsibleSection {
				Header = i.Name,
				Child = editor,
				Expanded = binding.ShouldBeSaved
			} );
		}
	}

	[Cached(name: "RulesetActions" )]
	public readonly List<object> RulesetActions;

	static List<IHasBindingType> getAvailableBindings () => new() {
		new ButtonBinding( Hand.Left ),
		new ButtonBinding( Hand.Right ),
		new JoystickBindings( Hand.Left ),
		new JoystickBindings( Hand.Right ),
		new ClapBinding()
	};
}
