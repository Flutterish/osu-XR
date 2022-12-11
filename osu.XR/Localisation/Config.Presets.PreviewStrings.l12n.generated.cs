// This file is auto-generated
// Do not edit it manually as it will be overwritten

using osu.Framework.Localisation;

namespace osu.XR.Localisation.Config.Presets {
	public static class PreviewStrings {
		private const string PREFIX = "osu.XR.Localisation.Config.Presets.Preview.Strings";
		private static string getKey( string key ) => $"{PREFIX}:{key}";

		/// <summary>
		/// Clone
		/// </summary>
		public static readonly LocalisableString Clone = new TranslatableString(
			getKey( "clone" ),
			"Clone"
		);

		/// <summary>
		/// {name} (Copy)
		/// </summary>
		public static LocalisableString CloneName( object name ) => new TranslatableString(
			getKey( "clone-name" ),
			"{0} (Copy)",
			name
		);

		/// <summary>
		/// Delete
		/// </summary>
		public static readonly LocalisableString Delete = new TranslatableString(
			getKey( "delete" ),
			"Delete"
		);

		/// <summary>
		/// Preset Preview
		/// </summary>
		public static readonly LocalisableString Header = new TranslatableString(
			getKey( "header" ),
			"Preset Preview"
		);

		/// <summary>
		/// Title
		/// </summary>
		public static readonly LocalisableString PresetName = new TranslatableString(
			getKey( "preset-name" ),
			"Title"
		);

		/// <summary>
		/// Revert defaults
		/// </summary>
		public static readonly LocalisableString Revert = new TranslatableString(
			getKey( "revert" ),
			"Revert defaults"
		);

		/// <summary>
		/// Save
		/// </summary>
		public static readonly LocalisableString Save = new TranslatableString(
			getKey( "save" ),
			"Save"
		);

		/// <summary>
		/// Preset settings
		/// </summary>
		public static readonly LocalisableString Settings = new TranslatableString(
			getKey( "settings" ),
			"Preset settings"
		);

		/// <summary>
		/// Toggle adding/removing items
		/// </summary>
		public static readonly LocalisableString ToggleItems = new TranslatableString(
			getKey( "toggle-items" ),
			"Toggle adding/removing items"
		);
	}
}
