using osu.Game.Configuration;
using System.Text.Json;

namespace osu.XR.Configuration.Presets;

public class ConfigurationPreset<TLookup> : InMemoryConfigManager<TLookup>, ITypedSettingSource<TLookup>, IPreset<ConfigurationPreset<TLookup>> where TLookup : struct, Enum {
	public Bindable<string> Name { get; } = new( string.Empty );
	public bool NeedsToBeSaved { get; set; } = true;

	public ConfigurationPreset () {
		Name.BindValueChanged( _ => NeedsToBeSaved = true );
	}

	public ConfigurationPreset ( ITypedSettingSource<TLookup> source, ConfigurationPresetLiteral<TLookup> literal ) : this() {
		Name.Value = literal.Name;
		foreach ( var (key, value) in literal.Values ) {
			source.TypedSettings[key].CopyTo( this, key );
			TypedSettings[key].Parse( value );
		}
	}

	public readonly BindableList<TLookup> Keys = new();
	public IReadOnlyDictionary<TLookup, ITypedSetting> TypedSettings => typedSettings;
	Dictionary<TLookup, ITypedSetting> typedSettings = new();
	protected override void AddBindable<TBindable> ( TLookup lookup, Bindable<TBindable> bindable ) {
		var typed = new TypedSetting<TBindable>( bindable );
		typedSettings.Add( lookup, typed );
		bindable.BindValueChanged( _ => {
			NeedsToBeSaved = true;
			SettingChanged?.Invoke( lookup, typed );
		} );
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

	public ConfigurationPreset<TLookup> Clone () {
		var clone = new ConfigurationPreset<TLookup>();
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

	public static ConfigurationPreset<TLookup> Deserialize ( Stream stream, ITypedSettingSource<TLookup> source ) {
		var preset = new ConfigurationPreset<TLookup>();
		var save = JsonSerializer.Deserialize<SaveData>( stream, new JsonSerializerOptions { IncludeFields = true } );

		preset.Name.Value = save.Name;
		foreach ( var (k, v) in save.Values ) {
			if ( !Enum.TryParse<TLookup>( k, out var key ) )
				continue;

			var value = (JsonElement)v;
			source.TypedSettings[key].CopyTo( preset, key );
			preset.TypedSettings[key].Parse( value.ToString() );
		}

		return preset;
	}

	public event Action<TLookup, ITypedSetting>? SettingChanged;
}

public class ConfigurationPresetLiteral<TLookup> where TLookup : struct, Enum {
	public string Name = string.Empty;
	public object? this[TLookup key] {
		set => Values[key] = value;
	}

	public Dictionary<TLookup, object?> Values = new();
}