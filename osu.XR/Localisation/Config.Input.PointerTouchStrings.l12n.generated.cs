// This file is auto-generated
// Do not edit it manually as it will be overwritten

using osu.Framework.Localisation;

namespace osu.XR.Localisation.Config.Input {
	public static class PointerTouchStrings {
		private const string PREFIX = "osu.XR.Localisation.Config.Input.PointerTouch.Strings";
		private static string getKey( string key ) => $"{PREFIX}:{key}";

		/// <summary>
		/// [Pointer] Emulate touch
		/// </summary>
		public static readonly LocalisableString Label = new TranslatableString(
			getKey( "label" ),
			"[Pointer] Emulate touch"
		);

		/// <summary>
		/// Emulate touch instead of mouse
		/// </summary>
		public static readonly LocalisableString Tooltip = new TranslatableString(
			getKey( "tooltip" ),
			"Emulate touch instead of mouse"
		);
	}
}
