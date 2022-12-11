using osu.Framework.Localisation;
using osu.XR.Localisation.Config.Input;

namespace osu.XR.Configuration;

public enum HandSetting {
	[LocalisableDescription(typeof(MainHandStrings), nameof(MainHandStrings.Auto))]
	Auto,

	[LocalisableDescription(typeof(MainHandStrings), nameof(MainHandStrings.Left))]
	Left,

	[LocalisableDescription(typeof(MainHandStrings), nameof(MainHandStrings.Right))]
	Right
}