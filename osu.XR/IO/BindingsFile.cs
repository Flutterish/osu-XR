using osu.Framework.Localisation;
using osu.XR.Input;

namespace osu.XR.IO;

public class BindingsFile : UniqueCompositeActionBinding<RulesetBindings, string> {
	public override LocalisableString Name => "osu!xr bindings";
	protected override string? GetKey ( RulesetBindings action ) => action.ShortName;
}
