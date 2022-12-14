using osu.Framework.Localisation;

namespace osu.XR.Input;

public class RulesetBindings : UniqueCompositeActionBinding<VariantBindings, int> {
	public override LocalisableString Name => ShortName;
	protected override int GetKey ( VariantBindings action ) => action.Variant;

	public readonly string ShortName;
	public RulesetBindings ( string shortName ) {
		ShortName = shortName;
	}
}
