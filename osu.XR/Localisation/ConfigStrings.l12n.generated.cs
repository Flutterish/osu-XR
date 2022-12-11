// This file is auto-generated
// Do not edit it manually as it will be overwritten

using osu.Framework.Localisation;

namespace osu.XR.Localisation {
	public static class ConfigStrings {
		private const string PREFIX = "osu.XR.Localisation.Config.Strings";
		private static string getKey( string key ) => $"{PREFIX}:{key}";

		/// <summary>
		/// change the way osu!XR behaves
		/// </summary>
		public static readonly LocalisableString Flavour = new TranslatableString(
			getKey( "flavour" ),
			"change the way osu!XR behaves"
		);

		/// <summary>
		/// VR Settings
		/// </summary>
		public static readonly LocalisableString Header = new TranslatableString(
			getKey( "header" ),
			"VR Settings"
		);
	}
}
