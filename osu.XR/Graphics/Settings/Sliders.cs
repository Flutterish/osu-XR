using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;

namespace osu.XR.Graphics.Settings;

public class PxSliderBar : OsuSliderBar<int> {
	public override LocalisableString TooltipText => $"{Current.Value}px";
}

public class RadToDegreeSliderBar : OsuSliderBar<float> {
	public override LocalisableString TooltipText => $"{Current.Value / Math.PI * 180:N0}°";
}

public class MetersSliderBar : OsuSliderBar<float> {
	public override LocalisableString TooltipText => $"{Current.Value:N2}m";
}

public class PercentSliderBar : OsuSliderBar<float> {
	public override LocalisableString TooltipText => $"{Current.Value:0%}";
}