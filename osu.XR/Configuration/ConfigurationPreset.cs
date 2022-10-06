using osu.Game.Configuration;

namespace osu.XR.Configuration;

public class ConfigurationPreset<Tlookup> : InMemoryConfigManager<Tlookup> where Tlookup : struct, Enum {
	public string Name = string.Empty;

	// scuffed generic indexer
	public dynamic? this[Tlookup lookup] {
		set => SetDefault( lookup, value );
	}

	protected override void AddBindable<TBindable> ( Tlookup lookup, Bindable<TBindable> bindable ) {
		Keys.Add( lookup );
		base.AddBindable( lookup, bindable );
	}

	new public IReadOnlyDictionary<Tlookup, IBindable> ConfigStore => base.ConfigStore;

	public readonly BindableList<Tlookup> Keys = new();
}
