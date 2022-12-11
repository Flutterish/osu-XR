// This file is auto-generated
// Do not edit it manually as it will be overwritten

using osu.Framework.Localisation;

namespace osu.XR.Localisation.Config.Presets {
	public static class ManageStrings {
		private const string PREFIX = "osu.XR.Localisation.Config.Presets.Manage.Strings";
		private static string getKey( string key ) => $"{PREFIX}:{key}";

		/// <summary>
		/// Create new preset
		/// </summary>
		public static readonly LocalisableString New = new TranslatableString(
			getKey( "new" ),
			"Create new preset"
		);

		/// <summary>
		/// New Preset
		/// </summary>
		public static readonly LocalisableString NewName = new TranslatableString(
			getKey( "new-name" ),
			"New Preset"
		);

		/// <summary>
		/// Create a new preset with current settings
		/// </summary>
		public static readonly LocalisableString NewTooltip = new TranslatableString(
			getKey( "new-tooltip" ),
			"Create a new preset with current settings"
		);
	}
}
