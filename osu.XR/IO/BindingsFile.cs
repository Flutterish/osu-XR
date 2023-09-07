using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.XR.Input;
using osu.XR.Input.Migration;
using System.Text.Json;
using System.Text.Json.Serialization;

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

	public static BindingsFile LoadFromStorage ( Storage storage, string filename, BindingsSaveContext ctx ) {
		if ( !storage.Exists( filename ) )
			return new();

		try {
			using var stream = storage.GetStream( filename, FileAccess.Read, FileMode.Open );
			var data = JsonSerializer.Deserialize<JsonElement>( stream );
			return Load( data, ctx ) ?? new();
		}
		catch ( Exception e ) {
			ctx.Error( @"Failed to load bindings", filename, e );
			return new();
		}
	}

	public void SaveToStorage ( Storage storage, string filename, BindingsSaveContext ctx ) {
		try {
			var opts = new JsonSerializerOptions {
				IncludeFields = true,
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
			};
			opts.Converters.Add( new JsonStringEnumConverter() );
			using var stream = storage.CreateFileSafely( filename );
			JsonSerializer.Serialize( stream, GetSaveData( ctx ), opts );
		}
		catch ( Exception e ) {
			ctx.Error( @"Failed to save bindings", filename, e );
		}
	}

	[FormatVersion( "[Initial]" )]
	[FormatVersion( "" )]
	public struct SaveData {
		public string Name;
		public string Description;
		public object[] Rulesets;
	}
}
