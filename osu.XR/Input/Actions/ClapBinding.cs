using osu.Framework.Localisation;
using osu.XR.Localisation.Bindings;

namespace osu.XR.Input.Actions;

public class ClapBinding : ActionBinding {
	public override LocalisableString Name => TypesStrings.Clap;

	public readonly Bindable<object> Action = new();
	public readonly Bindable<double> ThresholdABindable = new( 0.325 );
	public readonly Bindable<double> ThresholdBBindable = new( 0.275 );

	public ClapBinding () {
		TrackSetting( ThresholdABindable );
		TrackSetting( ThresholdBBindable );
		TrackSetting( Action );
	}
}
