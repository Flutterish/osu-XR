using osu.Framework.Localisation;
using osu.XR.Input;
using System.Text.Json;

namespace osu.XR.IO;

public class BindingsFile : UniqueCompositeActionBinding<RulesetBindings, string> {
	public override LocalisableString Name => "osu!xr bindings";
	protected override string GetKey ( RulesetBindings action ) => action.ShortName;

	public string FileName = "Ruleset bindings for osu!XR";
	public string Description = "Exported ruleset bindings";
	protected override object CreateSaveData ( IEnumerable<RulesetBindings> children, BindingsSaveContext context ) => new SaveData {
		Name = FileName,
		Description = Description,
		Rulesets = CreateSaveDataAsArray( children, context )
	};

	public static BindingsFile? Load ( JsonElement data, BindingsSaveContext ctx ) => Load<BindingsFile, SaveData>( data, ctx, static (save, ctx) => {
		var file = new BindingsFile {
			FileName = save.Name,
			Description = save.Description
		};
		file.LoadChildren( save.Rulesets, ctx, RulesetBindings.Load );
		return file;
	} );

	public struct SaveData {
		public string Name;
		public string Description;
		public object[] Rulesets;
	}
}
