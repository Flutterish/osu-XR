// This file is auto-generated
// Do not edit it manually as it will be overwritten

using osu.Framework.Localisation;

namespace osu.XR.Localisation {
	public static class UnitsStrings {
		private const string PREFIX = "osu.XR.Localisation.Units.Strings";
		private static string getKey( string key ) => $"{PREFIX}:{key}";

		/// <summary>
		/// {value:N0}°
		/// </summary>
		public static LocalisableString Degrees( object value ) => new TranslatableString(
			getKey( "degrees" ),
			"{0:N0}°",
			value
		);

		/// <summary>
		/// {value:N2}m
		/// </summary>
		public static LocalisableString Meters( object value ) => new TranslatableString(
			getKey( "meters" ),
			"{0:N2}m",
			value
		);

		/// <summary>
		/// {value:0%}
		/// </summary>
		public static LocalisableString Percent( object value ) => new TranslatableString(
			getKey( "percent" ),
			"{0:0%}",
			value
		);

		/// <summary>
		/// {value:N0}px
		/// </summary>
		public static LocalisableString Pixel( object value ) => new TranslatableString(
			getKey( "pixel" ),
			"{0:N0}px",
			value
		);
	}
}
