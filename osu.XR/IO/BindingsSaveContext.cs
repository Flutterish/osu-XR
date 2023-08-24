﻿using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Rulesets;
using osu.XR.Input.Actions;

namespace osu.XR.IO;

public class BindingsSaveContext {
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
		if ( data is not ActionData save )
			return false;
		action.NotLoaded = save;

		if ( !actions.TryGetValue( save.ID, out var byId ) ) {
			Warning( $@"Could not load action '{save.Name}' by ID", save );
		}
		if ( !names.TryGetValue( save.Name, out var byName ) ) {
			Warning( $@"Could not load action '{save.Name}' by name", save );
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

	public readonly BindableList<Message> Messages = new(); // TODO forward this to notifications
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