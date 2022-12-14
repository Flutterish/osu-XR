using osu.Framework.Localisation;
using osu.XR.Input;

namespace osu.XR.IO;

public class BindingsFile : UniqueCompositeActionBinding<RulesetBindings, string> {
	public override LocalisableString Name => "osu!xr bindings";
	protected override string GetKey ( RulesetBindings action ) => action.ShortName;

	protected override object CreateSaveData ( IEnumerable<RulesetBindings> children, BindingsSaveContext context ) => new SaveData {
		Name = "Ruleset bindings for osu!XR",
		Description = "Exported ruleset bindings",
		Rulesets = CreateSaveDataAsList( children, context )
	};

	public struct SaveData {
		public string Name;
		public string Description;
		public object Rulesets;
	}
}
