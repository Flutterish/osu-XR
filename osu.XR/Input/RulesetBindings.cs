using osu.Framework.Localisation;
using osu.Game.Rulesets;
using osu.XR.IO;

namespace osu.XR.Input;

public class RulesetBindings : UniqueCompositeActionBinding<VariantBindings, int> {
	public override LocalisableString Name => ShortName;
	protected override int GetKey ( VariantBindings action ) => action.Variant;
	
	Ruleset? ruleset;
	public Ruleset? Ruleset {
		get => ruleset;
		set {
			ruleset = value;
			// TODO load data
		}
	}

	public readonly string ShortName;
	public RulesetBindings ( string shortName ) {
		ShortName = shortName;
	}

	protected override object CreateSaveData ( IEnumerable<VariantBindings> children, BindingsSaveContext context ) => new SaveData {
		Name = ShortName,
		VariantNames = context.VariantsChecksum( Ruleset! ),
		Variants = CreateSaveDataAsDictionary( children, context )
	};

	public struct SaveData {
		public string Name;
		public Dictionary<int, string>? VariantNames;
		public Dictionary<int, object> Variants;
	}
}
