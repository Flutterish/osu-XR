using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;

namespace osu.XR.Graphics.Settings;

public partial class PxSliderBar : RoundedSliderBar<int> {
	public override LocalisableString TooltipText => Localisation.UnitsStrings.Pixel(Current.Value);
}

public partial class RadToDegreeSliderBar : RoundedSliderBar<float> {
	public override LocalisableString TooltipText => Localisation.UnitsStrings.Degrees(Current.Value / Math.PI * 180);
}

public partial class MetersSliderBar : RoundedSliderBar<float> {
	public override LocalisableString TooltipText => Localisation.UnitsStrings.Meters(Current.Value);
}

public partial class PercentSliderBar : RoundedSliderBar<float> {
	public override LocalisableString TooltipText => Localisation.UnitsStrings.Percent(Current.Value);
}