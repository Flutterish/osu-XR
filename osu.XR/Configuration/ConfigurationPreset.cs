using osu.Game.Configuration;

namespace osu.XR.Configuration;

public class ConfigurationPreset<Tlookup> : InMemoryConfigManager<Tlookup> where Tlookup : struct, Enum {
	public readonly Bindable<string> NameBindable = new( string.Empty );
	public string Name {
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

	Dictionary<Tlookup, Action> saveDefaults = new();
	Dictionary<Tlookup, Action> revertDefaults = new();
	protected override void AddBindable<TBindable> ( Tlookup lookup, Bindable<TBindable> bindable ) {
		base.AddBindable( lookup, bindable );
		saveDefaults[lookup] = () => bindable.Default = bindable.Value;
		revertDefaults[lookup] = bindable.SetDefault;
		Keys.Add( lookup );
	}

	public void Remove ( Tlookup lookup ) {
		base.ConfigStore.Remove( lookup );
		Keys.Remove( lookup );
	}

	public void RevertToDefault () {
		foreach ( var (_, i) in revertDefaults ) {
			i();
		}
	}
	public void SaveDefaults () {
		NameBindable.Default = Name;
		foreach ( var (_, i) in saveDefaults ) {
			i();
		}
	}

	new public IReadOnlyDictionary<Tlookup, IBindable> ConfigStore => base.ConfigStore;

	public readonly BindableList<Tlookup> Keys = new();
}
