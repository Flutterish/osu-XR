using osu.Framework.Localisation;
using osu.XR.Localisation.Scenery;

namespace osu.XR.Configuration;

public enum SceneryType {
	[LocalisableDescription(typeof(TypesStrings), nameof(TypesStrings.Solid))]
	Solid,

	[LocalisableDescription(typeof(TypesStrings), nameof(TypesStrings.LightsOut))]
	LightsOut
}