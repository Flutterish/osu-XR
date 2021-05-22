using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.XR.Input.Custom.Persistence {
	public sealed class RulesetBindingsFile {
		[JsonProperty( Order = 1 )]
		public string FormatVersion { get; init; } = "[Initial]";

		[JsonProperty( Order = 2 )]
		public string Name = "Ruleset bindings for osu!XR";
		[JsonProperty( Order = 3 )]
		public string Description = "Exported ruleset bindings";

		[JsonProperty( Order = 4 )]
		public readonly List<RulesetBindings> Rulesets = new();

		/// <summary>
		/// Performs a shallow merge where only the missing rulesets are added.
		/// </summary>
		public RulesetBindingsFile MergeWith ( RulesetBindingsFile other ) {
			foreach ( var i in other.Rulesets.Where( x => !Rulesets.Any( y => y.Name == x.Name ) ) ) {
				Rulesets.Add( i );
			}

			return this;
		}

		public static RulesetBindingsFile Load ( JToken token ) {
			var (version,root) = token.GetFormatVersion();

			if ( version == "[Initial]" ) {
				RulesetBindingsFile file = new() {
					FormatVersion = version,
					Name = (string)root[ "Name" ],
					Description = (string)root[ "Description" ]
				};

				foreach ( var ruleset in ( root[ "Rulesets" ] as JArray ) ) {
					file.Rulesets.Add( RulesetBindings.Load( ruleset ) );
				}

				return file;
			}
			else {
				throw new Exception( "Invalid format version" );
			}
		}
	}

	public static class JsonExtensions {
		public static (string version, JObject root) GetFormatVersion ( this JToken token ) {
			if ( token is not JObject root || root[ "FormatVersion" ] is not JToken version || version.Type != JTokenType.String )
				throw new Exception( "Could not find format version" );

			return ((string)version, root);
		}
	}
}
