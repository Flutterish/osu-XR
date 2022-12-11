using osu.Framework.Localisation;
using osu.XR.Localisation.Config.Input;

namespace osu.XR.Configuration;

public enum InputMode {
	[LocalisableDescription(typeof(ModeStrings), nameof(ModeStrings.Single))]
	SinglePointer,

	[LocalisableDescription(typeof(ModeStrings), nameof(ModeStrings.Double))]
	DoublePointer,

	[LocalisableDescription(typeof(ModeStrings), nameof(ModeStrings.Touch))]
	TouchScreen
}