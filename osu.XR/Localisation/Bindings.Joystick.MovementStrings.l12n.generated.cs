// This file is auto-generated
// Do not edit it manually as it will be overwritten

using osu.Framework.Localisation;

namespace osu.XR.Localisation.Bindings.Joystick {
	public static class MovementStrings {
		private const string PREFIX = "osu.XR.Localisation.Resx.Bindings.Joystick.Movement.Strings";
		private static string getKey( string key ) => $"{PREFIX}:{key}";

		/// <summary>
		/// Distance
		/// </summary>
		public static readonly LocalisableString Distance = new TranslatableString(
			getKey( "distance" ),
			"Distance"
		);

		/// <summary>
		/// Current implementation disables all input from outside the ruleset binding section.\nMake sure to bind your other buttons here too.
		/// </summary>
		public static readonly LocalisableString Warning = new TranslatableString(
			getKey( "warning" ),
			"Current implementation disables all input from outside the ruleset binding section.\\nMake sure to bind your other buttons here too."
		);
	}
}
