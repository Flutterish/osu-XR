using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace osu.XR.Input.Custom.Persistence {
	public sealed class BindingData {
		[JsonProperty( Order = 1 )]
		public string FormatVersion { get; init; } = "[Initial]";

		[JsonProperty( Order = 2 )]
		public string Type { get; init; }
		[JsonProperty( Order = 3 )]
		public object Data;

		public static BindingData Load ( JToken token ) {
			var (version, root) = token.GetFormatVersion();

			if ( version == "[Initial]" ) {
				return new() {
					FormatVersion = version,
					Type = (string)root[ "Type" ],
					Data = root[ "Data" ]
				};
			}
			else {
				throw new Exception( "Invalid format version" );
			}
		}
	}
}
