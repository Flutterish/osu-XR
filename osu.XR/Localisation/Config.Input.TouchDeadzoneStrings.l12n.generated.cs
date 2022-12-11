// This file is auto-generated
// Do not edit it manually as it will be overwritten

using osu.Framework.Localisation;

namespace osu.XR.Localisation.Config.Input {
	public static class TouchDeadzoneStrings {
		private const string PREFIX = "osu.XR.Localisation.Config.Input.TouchDeadzone.Strings";
		private static string getKey( string key ) => $"{PREFIX}:{key}";

		/// <summary>
		/// Touch deadzone
		/// </summary>
		public static readonly LocalisableString Label = new TranslatableString(
			getKey( "label" ),
			"Touch deadzone"
		);

		/// <summary>
		/// Deadzone after interacting with a panel 
		/// </summary>
		public static readonly LocalisableString Tooltip = new TranslatableString(
			getKey( "tooltip" ),
			"Deadzone after interacting with a panel "
		);
	}
}
