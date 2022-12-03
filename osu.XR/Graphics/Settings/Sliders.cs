using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;

namespace osu.XR.Graphics.Settings;

public partial class PxSliderBar : OsuSliderBar<int> {
	public override LocalisableString TooltipText => $"{Current.Value}px";
}

public partial class RadToDegreeSliderBar : OsuSliderBar<float> {
	public override LocalisableString TooltipText => $"{Current.Value / Math.PI * 180:N0}°";
}

public partial class MetersSliderBar : OsuSliderBar<float> {
	public override LocalisableString TooltipText => $"{Current.Value:N2}m";
}

public partial class PercentSliderBar : OsuSliderBar<float> {
	public override LocalisableString TooltipText => $"{Current.Value:0%}";
}