using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Input.Custom.Persistence {
	public sealed class RulesetBindings {
		[JsonProperty( Order = 1 )]
		public string FormatVersion { get; init; } = "[Initial]";

		[JsonProperty( Order = 2 )]
		public string Name { get; init; }

		[JsonProperty( Order = 3 )]
		public readonly Dictionary<int, string> VariantNames = new();
		[JsonProperty( Order = 4 )]
		public readonly Dictionary<int, RulesetVariantBindings> Variants = new();

		public static RulesetBindings Load ( JToken token ) {
			var (version, root) = token.GetFormatVersion();

			if ( version == "[Initial]" ) {
				RulesetBindings ruleset = new() {
					FormatVersion = version,
					Name = (string)root[ "Name" ]
				};

				foreach ( var (index, name) in root[ "VariantNames" ].ToObject<Dictionary<int, string>>() ) {
					ruleset.VariantNames.Add( index, name );
				}

				foreach ( var (index, variant) in root[ "Variants" ].ToObject<Dictionary<int, object>>() ) {
					ruleset.Variants.Add( index, RulesetVariantBindings.Load( variant as JToken ) );
				}

				return ruleset;
			}
			else {
				throw new Exception( "Invalid ruleset format version" );
			}
		}
	}
}
