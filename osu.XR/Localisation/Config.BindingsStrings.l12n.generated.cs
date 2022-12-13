// This file is auto-generated
// Do not edit it manually as it will be overwritten

using osu.Framework.Localisation;

namespace osu.XR.Localisation.Config {
	public static class BindingsStrings {
		private const string PREFIX = "osu.XR.Localisation.Resx.Config.Bindings.Strings";
		private static string getKey( string key ) => $"{PREFIX}:{key}";

		/// <summary>
		/// change how you play {ruleset} in VR
		/// </summary>
		public static LocalisableString Flavour( object ruleset ) => new TranslatableString(
			getKey( "flavour" ),
			"change how you play {0} in VR",
			ruleset
		);

		/// <summary>
		/// change how you play in VR
		/// </summary>
		public static readonly LocalisableString FlavourNone = new TranslatableString(
			getKey( "flavour-none" ),
			"change how you play in VR"
		);

		/// <summary>
		/// Ruleset
		/// </summary>
		public static readonly LocalisableString Header = new TranslatableString(
			getKey( "header" ),
			"Ruleset"
		);

		/// <summary>
		/// Variant
		/// </summary>
		public static readonly LocalisableString Variant = new TranslatableString(
			getKey( "variant" ),
			"Variant"
		);
	}
}
