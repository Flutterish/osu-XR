using osu.Framework.Localisation;
using osu.Game.Rulesets;
using osu.XR.IO;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace osu.XR.Input;

public class RulesetBindings : UniqueCompositeActionBinding<VariantBindings, int> {
	public override LocalisableString Name => ShortName;
	protected override int GetKey ( VariantBindings action ) => action.Variant;
	
	Ruleset? ruleset;
	[DisallowNull]
	public Ruleset? Ruleset {
		get => ruleset;
		set {
			ruleset = value;
			if ( toBeLoaded is not SaveData save )
				return;

			var ctx = new BindingsSaveContext();
			var checksum = ctx.VariantsChecksum( value );
			// TODO validate variants
			LoadChildren( save.Variants, ctx, VariantBindings.Load );

			toBeLoaded = null;
		}
	}

	SaveData? toBeLoaded;
	public string ShortName { get; private set; }
	public RulesetBindings ( string shortName ) {
		ShortName = shortName;
	}
	public static RulesetBindings? Load ( JsonElement data, BindingsSaveContext ctx ) => Load<RulesetBindings, SaveData>( data, ctx, static (save, ctx) => {
		return new RulesetBindings( save.Name ) {
			toBeLoaded = save
		};
	} );

	protected override object CreateSaveData ( IEnumerable<VariantBindings> children, BindingsSaveContext context ) => Ruleset != null ? new SaveData {
		Name = ShortName,
		VariantNames = context.VariantsChecksum( Ruleset ),
		Variants = CreateSaveDataAsDictionary( children, context )
	} : toBeLoaded ?? default;

	public struct SaveData {
		public string Name;
		public Dictionary<int, string>? VariantNames;
		public Dictionary<int, object> Variants;
	}
}
