using osu.Game.Configuration;

namespace osu.XR.Configuration;

public class ConfigurationPreset<Tlookup> : InMemoryConfigManager<Tlookup> where Tlookup : struct, Enum {
	public string Name = string.Empty;

	// scuffed generic indexer
	public dynamic this[Tlookup lookup] {
		set => SetDefault( lookup, value );
	}

	new public IReadOnlyDictionary<Tlookup, IBindable> ConfigStore => base.ConfigStore;
}
