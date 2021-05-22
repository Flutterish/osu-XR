using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace osu.XR.Input.Custom.Persistence {
	public sealed class RulesetVariantBindings {
		[JsonProperty( Order = 1 )]
		public string FormatVersion { get; init; } = "[Initial]";

		[JsonProperty( Order = 2 )]
		public string Name { get; init; }

		[JsonProperty( Order = 3 )]
		public readonly Dictionary<int, string> Actions = new();
		[JsonProperty( Order = 4 )]
		public readonly List<BindingData> Bindings = new();

		public static RulesetVariantBindings Load ( JToken token ) {
			var (version, root) = token.GetFormatVersion();

			if ( version == "[Initial]" ) {
				RulesetVariantBindings variant = new() {
					FormatVersion = version,
					Name = (string)root[ "Name" ]
				};

				foreach ( var (index,name) in root[ "Actions" ].ToObject<Dictionary<int, string>>() ) {
					variant.Actions.Add( index, name );
				}

				foreach ( var binding in root[ "Bindings" ] as JArray ) {
					variant.Bindings.Add( BindingData.Load( binding ) );
				}

				return variant;
			}
			else {
				throw new Exception( "Invalid format version" );
			}
		}
	}
}
