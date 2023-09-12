using osu.Game.Configuration;
using System.Text.Json;

namespace osu.XR.Configuration;

public class ConfigurationPreset<Tlookup> : InMemoryConfigManager<Tlookup> where Tlookup : struct, Enum {
	public readonly Bindable<string> NameBindable = new( string.Empty );
	public string Name { // TODO figure out a way to integrate localisable and user-provided strings here
		get => NameBindable.Value;
		set {
			NameBindable.Default = value;
			NameBindable.Value = value;
		}
	}

	// scuffed generic indexer
	public dynamic? this[Tlookup lookup] {
		set => SetDefault( lookup, value );
	}

	Dictionary<Tlookup, Action<string>> setters = new();
	Dictionary<Tlookup, Func<object>> getters = new();
	Dictionary<Tlookup, Action> saveDefaults = new();
	Dictionary<Tlookup, Action> revertDefaults = new();
	Dictionary<Tlookup, Action<ConfigurationPreset<Tlookup>>> copy = new();
	protected override void AddBindable<TBindable> ( Tlookup lookup, Bindable<TBindable> bindable ) {
		base.AddBindable( lookup, bindable );
		saveDefaults[lookup] = () => bindable.Default = bindable.Value;
		revertDefaults[lookup] = bindable.SetDefault;
		getters[lookup] = () => bindable.Value;
		setters[lookup] = s => bindable.Parse( s );
		copy[lookup] = clone => clone[lookup] = Get<TBindable>( lookup );
		Keys.Add( lookup );

		bindable.BindValueChanged( v => SettingChanged?.Invoke( lookup, v.NewValue ) );
	}

	public void Remove ( Tlookup lookup ) {
		base.ConfigStore.Remove( lookup );
		Keys.Remove( lookup );
	}

	public void RevertToDefault () {
		foreach ( var (_, i) in revertDefaults ) {
			i();
		}
		NameBindable.SetDefault();
	}
	public void SaveDefaults () {
		NameBindable.Default = Name;
		foreach ( var (_, i) in saveDefaults ) {
			i();
		}
	}

	public virtual ConfigurationPreset<Tlookup> Clone () {
		var clone = new ConfigurationPreset<Tlookup>();
		clone.Name = Name;
		foreach ( var i in Keys ) {
			copy[i]( clone );
		}

		return clone;
	}
	
	struct SaveData {
		public string Name;
		public Dictionary<string, object> Values;
	}
	public string Stringify () {
		return JsonSerializer.Serialize( new SaveData {
			Name = Name,
			Values = Keys.ToDictionary( k => k.ToString(), k => getters[k]() )
		}, new JsonSerializerOptions { IncludeFields = true, WriteIndented = true } );
	}

	public void Parse ( string data ) {
		var save = JsonSerializer.Deserialize<SaveData>( data, new JsonSerializerOptions { IncludeFields = true } );
		Name = save.Name;
		Keys.Clear();
		foreach ( var (k, v) in save.Values ) {
			if ( !Enum.TryParse<Tlookup>( k, out var key ) )
				continue;

			var value = (JsonElement)v;
			setters[key]( value.ToString() );
			Keys.Add( key );
		}
	}

	new public IReadOnlyDictionary<Tlookup, IBindable> ConfigStore => base.ConfigStore;

	public readonly BindableList<Tlookup> Keys = new();

	public event Action<Tlookup, object>? SettingChanged;
}
