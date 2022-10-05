namespace osu.XR.Configuration;

public class ConfigurationPreset<T> : Dictionary<T, (object value, Type type)> where T : struct, Enum {
	 
}
