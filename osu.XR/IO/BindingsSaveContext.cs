using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Rulesets;
using osu.XR.Input.Actions;
using osu.XR.Input.Migration;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace osu.XR.IO;

public class BindingsSaveContext {
	public static readonly JsonSerializerOptions DefaultOptions;
	static BindingsSaveContext () {
		DefaultOptions = new JsonSerializerOptions {
			IncludeFields = true,
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};
		DefaultOptions.Converters.Add( new JsonStringEnumConverter() );
	}

	public Ruleset? Ruleset;
	public int? Variant;

	public BindingsSaveContext SetVaraint ( int value ) {
		Variant = value;
		return this;
	}
	public BindingsSaveContext SetRuleset ( Ruleset? value ) {
		Ruleset = value;
		Variant = null;
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

	public bool LoadAction ( RulesetAction action, ActionData? data ) {
		if ( data is not ActionData save || save.Name == null || save.ID < 0 )
			return false;
		action.NotLoaded = save;

		if ( !actions.TryGetValue( save.ID, out var byId ) ) {
			//Warning( $@"Could not load action '{save.Name}' by ID", save );
		}
		if ( !names.TryGetValue( save.Name, out var byName ) ) {
			//Warning( $@"Could not load action '{save.Name}' by name", save );
		}
		if ( byId == byName ) {
			if ( byId == null ) {
				Error( $@"Could not load action '{save.Name}'", save );
				return false;
			}
			action.Value = byName;
			return true;
		}
		if ( byName != null ) {
			Warning( $@"Mismatched action name and ID, loading '{save.Name}' based on name", save );
			action.Value = byName;
			return true;
		}
		else {
			if ( byId!.ToString() != save.Name )
				Warning( $@"Action '{save.Name}' loaded by ID has mismatched name ({byId})", save );
			action.Value = byId;
			return true;
		}
	}

	Migrant migrant = new();
	public bool DeserializeBindingData<T> ( JsonElement json, [NotNullWhen( true )] out T? value, JsonSerializerOptions? options = null ) {
		if ( typeof( T ) == typeof( JsonElement ) ) {
			value = (T)(object)json;
			return true;
		}

		options ??= DefaultOptions;
		var versionContainer = json.Deserialize<VersionContainer>( options );

		var migrator = migrant.GetMigrator<T>( versionContainer.FormatVersion );
		if ( migrator == null ) {
			Error( $@"Invalid binding version for `{typeof(T).ReadableName()}` - `{versionContainer.FormatVersion}`", json );
			value = default;
			return false;
		}

		var loaded = json.Deserialize( migrator.Value.detectedType, options )!;
		if ( !validateBindingData( migrator.Value.detectedType, loaded ) ) {
			value = default;
			return false;
		}

		value = (T)migrator.Value.migrator( loaded );
		return true;
	}

	bool validateBindingData ( Type type, object value ) {
		foreach ( var i in type.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) ) {
			if ( !i.IsNullable() && i.GetValue( value ) == null ) {
				Error( $@"Invalid binding data - '{i.Name}' must be present", value );
				return false;
			}
		}
		foreach ( var i in type.GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ).Where( x => x.CanRead && x.GetIndexParameters().Length == 0 ) ) {
			if ( !i.IsNullable() && i.GetValue( value ) == null ) {
				Error( $@"Invalid binding data - '{i.Name}' must be present", value );
				return false;
			}
		}
		return true;
	}

	struct VersionContainer {
		public string? FormatVersion;
	}

	public Dictionary<int, string> VariantsChecksum ( Ruleset ruleset ) {
		SetRuleset( ruleset );
		return ruleset.AvailableVariants.ToDictionary( x => x, x => ruleset.GetVariantName(x).ToString() );
	}

	public string VariantName ( int variant )
		=> Ruleset!.GetVariantName( variant ).ToString();

	Dictionary<int, object> actions = null!;
	Dictionary<object, int> indices = null!;
	Dictionary<string, object> names = null!;
	public Dictionary<int, string> ActionsChecksum ( int variant ) {
		SetVaraint( variant );

		actions = Ruleset!.GetDefaultKeyBindings( variant ).Select( x => x.Action ).Distinct().Select( ( x, i ) => (x, i) )
			.ToDictionary( x => x.i, x => x.x );
		names = actions.ToDictionary( x => x.Value.ToString()!, x => x.Value );
		indices = actions.ToDictionary( x => x.Value, x => x.Key );
		return actions.ToDictionary( x => x.Key, x => x.Value.ToString()! );
	}

	public readonly BindableList<Message> Messages = new();
	public void Log ( LocalisableString text, object? context = null ) {
		Messages.Add( new() { Severity = Severity.Log, Text = text, Context = context, Ruleset = Ruleset, Variant = Variant } );
		Logger.Log( $"{text} - {Ruleset} (Variant {Variant}) - {context}", "osu!xr-runtime" );
	}
	public void Warning ( LocalisableString text, object? context = null ) {
		Messages.Add( new() { Severity = Severity.Warning, Text = text, Context = context, Ruleset = Ruleset, Variant = Variant } );
		Logger.Log( $"{text} - {Ruleset} (Variant {Variant}) - {context}", "osu!xr-runtime", level: LogLevel.Important );
	}
	public void Error ( LocalisableString text, object? context = null, Exception? exception = null ) {
		Messages.Add( new() { Severity = Severity.Error, Text = text, Context = context, Ruleset = Ruleset, Variant = Variant, Exception = exception } );
		if ( exception != null )
			Logger.Error( exception, $"{text} - {Ruleset} (Variant {Variant}) - {context}", "osu!xr-runtime", recursive: true );
		else
			Logger.Log( $"{text} - {Ruleset} (Variant {Variant}) - {context}", "osu!xr-runtime", level: LogLevel.Error );
	}

	public struct Message {
		public Severity Severity;
		public LocalisableString Text;
		public object? Context;
		public Exception? Exception;
		public Ruleset? Ruleset;
		public int? Variant;
	}
}

public struct ActionData {
	public string Name;
	public int ID;
}

public enum Severity {
	Log,
	Warning,
	Error
}