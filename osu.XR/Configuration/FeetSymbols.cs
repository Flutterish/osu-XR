using osu.Framework.Localisation;
using osu.XR.Localisation.Config.Graphics;

namespace osu.XR.Configuration;

public enum FeetSymbols {
	[LocalisableDescription(typeof(ShadowStrings), nameof(ShadowStrings.None))]
	None,

	[LocalisableDescription(typeof(ShadowStrings), nameof(ShadowStrings.Feet))]
	Shoes,

	[LocalisableDescription(typeof(ShadowStrings), nameof(ShadowStrings.Paws))]
	Paws
}