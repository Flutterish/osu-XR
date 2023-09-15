using osu.Game.Configuration;
using System.Text.Json;

namespace osu.XR.Configuration.Presets;

public class ConfigPreset<TLookup> : InMemoryConfigManager<TLookup>, ITypedSettingSource<TLookup>, IPreset<ConfigPreset<TLookup>> where TLookup : struct, Enum {
	public Bindable<string> Name { get; } = new( string.Empty );
	public bool NeedsToBeSaved { get; set; } = true;

	public ConfigPreset () {
		Name.BindValueChanged( _ => NeedsToBeSaved = true );
	}

	public ConfigPreset ( IReadOnlyTypedSettingSource<TLookup> source, ConfigurationPresetLiteral<TLookup> literal ) : this() {
		Name.Value = literal.Name;
		foreach ( var (key, value) in literal.Values ) {
			source.TypedSettings[key].CopyTo( this, key );
			TypedSettings[key].Parse( value );
		}

		SaveDefaults();
	}

	public readonly BindableList<TLookup> Keys = new();
	public IReadOnlyDictionary<TLookup, ITypedSetting> TypedSettings => typedSettings;
	Dictionary<TLookup, ITypedSetting> typedSettings = new();
	protected override void AddBindable<TBindable> ( TLookup lookup, Bindable<TBindable> bindable ) {
		var typed = new TypedSetting<TBindable>( bindable );
		typedSettings.Add( lookup, typed );
		bindable.BindValueChanged( _ => NeedsToBeSaved = true );
		base.AddBindable( lookup, bindable );
		Keys.Add( lookup );
	}
	public void AddTypedSetting<TValue> ( TLookup key, TValue value ) {
		SetDefault( key, value );
	}

	public void SetTypedSetting<TValue> ( TLookup key, TValue value ) {
		SetValue( key, value );
	}
	public void RemoveTypedSetting ( TLookup key ) {
		if ( ConfigStore.Remove( key ) ) {
			NeedsToBeSaved = true;
			typedSettings.Remove( key );
			Keys.Remove( key );
		}
	}

	public void RevertToDefault () {
		foreach ( var (_, setting) in TypedSettings ) {
			setting.RevertToDefault();
		}
		Name.SetDefault();
	}
	public void SaveDefaults () {
		Name.Default = Name.Value;
		foreach ( var (_, setting) in TypedSettings ) {
			setting.SaveDefault();
		}
	}

	public ConfigPreset<TLookup> Clone () {
		var clone = new ConfigPreset<TLookup>();
		clone.Name.Value = Name.Value;
		foreach ( var (key, setting) in TypedSettings ) {
			setting.CopyTo( clone, key );
		}

		return clone;
	}

	struct SaveData {
		public string Name;
		public Dictionary<string, object> Values;
	}

	public void Serialize ( Stream stream ) {
		JsonSerializer.Serialize( stream, new SaveData {
			Name = Name.Value,
			Values = TypedSettings.ToDictionary( x => x.Key.ToString(), x => x.Value.GetValue()! )
		}, new JsonSerializerOptions { IncludeFields = true, WriteIndented = true } );
	}

	public static ConfigPreset<TLookup> Deserialize ( Stream stream, IReadOnlyTypedSettingSource<TLookup> source ) {
		var preset = new ConfigPreset<TLookup>();
		var save = JsonSerializer.Deserialize<SaveData>( stream, new JsonSerializerOptions { IncludeFields = true } );

		preset.Name.Value = save.Name;
		foreach ( var (k, v) in save.Values ) {
			if ( !Enum.TryParse<TLookup>( k, out var key ) )
				continue;

			var value = (JsonElement)v;
			source.TypedSettings[key].CopyTo( preset, key );
			preset.TypedSettings[key].Parse( value.ToString() );
		}

		preset.SaveDefaults();
		return preset;
	}
}

public class ConfigurationPresetLiteral<TLookup> where TLookup : struct, Enum {
	public string Name = string.Empty;
	public object? this[TLookup key] {
		set => Values[key] = value;
	}

	public Dictionary<TLookup, object?> Values = new();
}