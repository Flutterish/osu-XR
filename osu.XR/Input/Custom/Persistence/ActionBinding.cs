using Newtonsoft.Json;

namespace osu.XR.Input.Custom.Persistence {
	public sealed class ActionBinding {
		[JsonProperty( Order = 1 )]
		public string Name { get; init; }
		[JsonProperty( Order = 2 )]
		public int ID { get; init; }
	}
}
