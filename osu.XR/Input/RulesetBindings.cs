using osu.Framework.Localisation;
using osu.Game.Rulesets;
using osu.XR.Input.Migration;
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

			var checksum = ctx!.VariantsChecksum( value );
			var declared = save.VariantNames;
			if ( declared is null ) {
				ctx.Warning( @"Ruleset does not have a variant checksum", save );
			}
			else {
				var expected = $@"save declared {string.Join(", ", declared.Select( x => $@"(ID {x.Key} = {x.Value})" ))}, but it was {string.Join( ", ", checksum.Select( x => $@"(ID {x.Key} = {x.Value})" ) )}";

				if ( declared.Values.Except( checksum.Values ).Any() ) {
					ctx.Warning( $@"Ruleset action checksum has non-existent variants - {expected}", save );
				}
				else if ( declared.Except( checksum ).Any() ) {
					ctx.Warning( $@"Ruleset action action checksum is invalid - {expected}" , save );
				}
				// TODO try to fix this?
			}

			LoadChildren( save.Variants, ctx, VariantBindings.Load );

			toBeLoaded = null;
		}
	}

	SaveData? toBeLoaded;
	BindingsSaveContext? ctx;
	public BindingsSaveContext? SaveContext => ctx;
	public string ShortName { get; private set; }
	public RulesetBindings ( string shortName ) {
		ShortName = shortName;
	}

	public override bool ShouldBeSaved => base.ShouldBeSaved || toBeLoaded != null;

	public static RulesetBindings? Load ( JsonElement data, BindingsSaveContext ctx ) => Load<RulesetBindings, SaveData>( data, ctx, static (save, ctx) => {
		return new RulesetBindings( save.Name ) {
			toBeLoaded = save,
			ctx = ctx
		};
	} );

	protected override object CreateSaveData ( IEnumerable<VariantBindings> children, BindingsSaveContext context ) => Ruleset != null ? new SaveData {
		Name = ShortName,
		VariantNames = context.VariantsChecksum( Ruleset ),
		Variants = CreateSaveDataAsDictionary( children, context )
	} : toBeLoaded ?? default;

	[FormatVersion( "" )]
	[FormatVersion( "[Initial]" )]
	public struct SaveData {
		public string Name;
		public Dictionary<int, string>? VariantNames;
		public Dictionary<int, object> Variants;
	}
}
