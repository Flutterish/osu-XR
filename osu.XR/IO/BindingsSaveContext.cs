using osu.Game.Rulesets;

namespace osu.XR.IO;

public class BindingsSaveContext {
	public Ruleset? Ruleset;
	public int Variant;

	public BindingsSaveContext SetVaraint ( int value ) {
		Variant = value;
		return this;
	}
	public BindingsSaveContext SetRuleset ( Ruleset? value ) {
		Ruleset = value;
		return this;
	}

	public ActionData? SaveAction ( Bindable<object?> action )
		=> SaveAction( action.Value );
	public ActionData? SaveAction ( object? action ) {
		if ( action is null )
			return null;

		return new() {
			Name = action.ToString()!,
			ID = indices[action]
		};
	}

	public object? LoadAction ( ActionData? data ) {
		if ( data is not ActionData save )
			return null;
		
		// TODO check name integrity
		return actions.TryGetValue( save.ID, out var action ) ? action : null;
	}

	public Dictionary<int, string> VariantsChecksum ( Ruleset ruleset ) {
		Ruleset = ruleset;
		return ruleset.AvailableVariants.ToDictionary( x => x, x => ruleset.GetVariantName(x).ToString() );
	}

	public string VariantName ( int variant )
		=> Ruleset!.GetVariantName( variant ).ToString();

	Dictionary<int, object> actions = null!;
	Dictionary<object, int> indices = null!;
	public Dictionary<int, string> ActionsChecksum ( int variant ) {
		Variant = variant;

		actions = Ruleset!.GetDefaultKeyBindings( variant ).Select( x => x.Action ).Distinct().Select( ( x, i ) => (x, i) )
			.ToDictionary( x => x.i, x => x.x );
		indices = actions.ToDictionary( x => x.Value, x => x.Key );
		return actions.ToDictionary( x => x.Key, x => x.Value.ToString()! );
	}
}

public struct ActionData {
	public string Name;
	public int ID;
}